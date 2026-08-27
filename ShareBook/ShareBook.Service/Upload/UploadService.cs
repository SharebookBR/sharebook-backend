using Microsoft.Extensions.Options;
using ShareBook.Helper.Image;
using ShareBook.Service.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ShareBook.Service.Upload
{
    public class UploadService : IUploadService
    {
        private const string BooksDirectory = "Books";
        private const string BookThumbnailsDirectory = "Thumbs";

        private readonly ImageSettings _imageSettings;
        private readonly ServerSettings _serverSettings;

        public UploadService(IOptions<ImageSettings> imageSettings, IOptions<ServerSettings> serverSettings)
        {
            _imageSettings = imageSettings.Value;
            _serverSettings = serverSettings.Value;
        }

        public string GetImageUrl(string imageName, string lastDirectory)
        {
            if (string.IsNullOrWhiteSpace(imageName))
                return null;

            var dinamicDirectory = _imageSettings.ImagePath + "/" + lastDirectory;
            return ImageHelper.GenerateImageUrl(imageName, dinamicDirectory, _serverSettings.BackendUrl);
        }

        public string GetBookThumbnailUrl(string imageName)
        {
            var thumbnailName = ImageHelper.FormatThumbnailName(imageName);
            var directory = Path.Combine(BooksDirectory, BookThumbnailsDirectory).Replace('\\', '/');
            return GetImageUrl(thumbnailName, directory);
        }

        public async Task<string> UploadImageAsync(byte[] imageBytes, string imageName, string lastDirectory)
        {
            var dinamicDirectory = Path.Combine(_imageSettings.ImagePath, lastDirectory);

            byte[] thumbnailBytes = null;
            string thumbnailName = null;
            if (IsBooksDirectory(lastDirectory))
            {
                // Gera antes de escrever o original para que uma imagem inválida não deixe upload parcial.
                thumbnailBytes = ImageHelper.CreateBookThumbnail(imageBytes);
                thumbnailName = ImageHelper.FormatThumbnailName(imageName);
            }

            await UploadFileAsync(imageBytes, imageName, dinamicDirectory);

            if (thumbnailBytes != null)
            {
                var thumbnailDirectory = Path.Combine(dinamicDirectory, BookThumbnailsDirectory);
                await UploadFileAsync(thumbnailBytes, thumbnailName, thumbnailDirectory);
            }

            return GetImageUrl(imageName, lastDirectory);
        }

        public async Task<string> UploadPdfAsync(byte[] imageBytes, string imageName, string lastDirectory)
        {
            var dinamicDirectory = Path.Combine(_imageSettings.EBookPdfPath, lastDirectory);

            await UploadFileAsync(imageBytes, imageName, dinamicDirectory);

            return Path.Combine(lastDirectory, dinamicDirectory.Replace("wwwroot", ""), imageName);
        }

        public Task DeleteFileIfExistsAsync(string fileName, string lastDirectory)
        {
            if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(lastDirectory))
                return Task.CompletedTask;

            var dynamicDirectory = Path.Combine(_imageSettings.ImagePath, lastDirectory);
            var directoryBase = GetAbsoluteDirectory(dynamicDirectory);
            var filePath = Path.Combine(directoryBase, fileName);

            if (File.Exists(filePath))
                File.Delete(filePath);

            if (IsBooksDirectory(lastDirectory))
            {
                var thumbnailPath = Path.Combine(
                    directoryBase,
                    BookThumbnailsDirectory,
                    ImageHelper.FormatThumbnailName(fileName));

                if (File.Exists(thumbnailPath))
                    File.Delete(thumbnailPath);
            }

            return Task.CompletedTask;
        }

        public async Task<BookThumbnailBackfillResult> BackfillBookThumbnailsAsync(
            bool overwrite = false,
            int offset = 0,
            int batchSize = 50,
            CancellationToken cancellationToken = default)
        {
            var booksDirectory = GetAbsoluteDirectory(Path.Combine(_imageSettings.ImagePath, BooksDirectory));
            var thumbnailsDirectory = Path.Combine(booksDirectory, BookThumbnailsDirectory);
            Directory.CreateDirectory(thumbnailsDirectory);

            var sourceFiles = Directory.Exists(booksDirectory)
                ? Directory.EnumerateFiles(booksDirectory, "*", SearchOption.TopDirectoryOnly)
                    .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                    .ToList()
                : new List<string>();

            var normalizedOffset = Math.Max(offset, 0);
            var normalizedBatchSize = Math.Clamp(batchSize, 1, 200);
            var duplicateThumbnailNames = sourceFiles
                .GroupBy(
                    path => ImageHelper.FormatThumbnailName(Path.GetFileName(path)),
                    StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var batch = sourceFiles
                .Skip(normalizedOffset)
                .Take(normalizedBatchSize)
                .ToList();

            var result = new BookThumbnailBackfillResult
            {
                SourceFiles = sourceFiles.Count,
                Offset = normalizedOffset,
                Processed = batch.Count,
                HasMore = normalizedOffset + batch.Count < sourceFiles.Count,
                NextOffset = normalizedOffset + batch.Count < sourceFiles.Count
                    ? normalizedOffset + batch.Count
                    : null
            };

            foreach (var sourcePath in batch)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var sourceInfo = new FileInfo(sourcePath);
                var thumbnailName = ImageHelper.FormatThumbnailName(sourceInfo.Name);
                var thumbnailPath = Path.Combine(thumbnailsDirectory, thumbnailName);
                var thumbnailExists = File.Exists(thumbnailPath);

                if (duplicateThumbnailNames.Contains(thumbnailName))
                {
                    result.Failures.Add(new BookThumbnailBackfillFailure
                    {
                        FileName = sourceInfo.Name,
                        Error = $"Mais de uma capa de origem geraria o thumbnail {thumbnailName}."
                    });
                    continue;
                }

                if (!overwrite
                    && thumbnailExists
                    && File.GetLastWriteTimeUtc(thumbnailPath) >= sourceInfo.LastWriteTimeUtc)
                {
                    result.Skipped++;
                    result.SourceBytes += sourceInfo.Length;
                    result.ThumbnailBytes += new FileInfo(thumbnailPath).Length;
                    continue;
                }

                try
                {
                    var sourceBytes = await File.ReadAllBytesAsync(sourcePath, cancellationToken);
                    var thumbnailBytes = ImageHelper.CreateBookThumbnail(sourceBytes);
                    await WriteFileAtomicallyAsync(thumbnailPath, thumbnailBytes, cancellationToken);

                    result.SourceBytes += sourceBytes.Length;
                    result.ThumbnailBytes += thumbnailBytes.Length;
                    if (thumbnailExists)
                        result.Updated++;
                    else
                        result.Created++;
                }
                catch (Exception ex)
                {
                    result.Failures.Add(new BookThumbnailBackfillFailure
                    {
                        FileName = sourceInfo.Name,
                        Error = ex.Message
                    });
                }
            }

            return result;
        }

        private static async Task UploadFileAsync(byte[] imageBytes, string imageName, string dinamicDirectory)
        {
            var directoryBase = GetAbsoluteDirectory(dinamicDirectory);
            if (!Directory.Exists(directoryBase))
                Directory.CreateDirectory(directoryBase);

            var imageCompletePath = Path.Combine(directoryBase, imageName);
            await File.WriteAllBytesAsync(imageCompletePath, imageBytes);
        }

        private static async Task WriteFileAtomicallyAsync(
            string filePath,
            byte[] bytes,
            CancellationToken cancellationToken)
        {
            var temporaryPath = $"{filePath}.{Guid.NewGuid():N}.tmp";

            try
            {
                await File.WriteAllBytesAsync(temporaryPath, bytes, cancellationToken);
                File.Move(temporaryPath, filePath, true);
            }
            finally
            {
                if (File.Exists(temporaryPath))
                    File.Delete(temporaryPath);
            }
        }

        private static string GetAbsoluteDirectory(string directory)
            => Path.IsPathRooted(directory)
                ? directory
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directory);

        private static bool IsBooksDirectory(string directory)
            => string.Equals(
                directory?.TrimEnd('/', '\\'),
                BooksDirectory,
                StringComparison.OrdinalIgnoreCase);
    }
}
