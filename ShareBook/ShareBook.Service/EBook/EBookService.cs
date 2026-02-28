using Microsoft.Extensions.Options;
using ShareBook.Domain;
using ShareBook.Domain.Enums;
using ShareBook.Domain.Exceptions;
using ShareBook.Service.Server;
using ShareBook.Service.Upload;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ShareBook.Service.EBook
{
    public class EBookService : IEBookService
    {
        private readonly ImageSettings _imageSettings;
        private readonly ServerSettings _serverSettings;
        private readonly EBookStorageSettings _storageSettings;
        private readonly IS3Service _s3Service;

        public EBookService(
            IOptions<ImageSettings> imageSettings,
            IOptions<ServerSettings> serverSettings,
            IOptions<EBookStorageSettings> storageSettings,
            IS3Service s3Service)
        {
            _imageSettings = imageSettings.Value;
            _serverSettings = serverSettings.Value;
            _storageSettings = storageSettings.Value;
            _s3Service = s3Service;
        }

        public async Task<string> UploadPdfAsync(Book book)
        {
            if (!book.HasPdfToUpload())
                return null;

            if (_storageSettings.UseLocalStorage)
                return await UploadLocalAsync(book);

            return await UploadS3Async(book);
        }

        private async Task<string> UploadLocalAsync(Book book)
        {
            var pdfFileName = book.GetPdfFileName();
            var fullDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _imageSettings.EBookPdfPath);

            Directory.CreateDirectory(fullDirectoryPath);

            var pdfFullPath = Path.Combine(fullDirectoryPath, pdfFileName);
            await File.WriteAllBytesAsync(pdfFullPath, book.PdfBytes);

            return pdfFileName;
        }

        private async Task<string> UploadS3Async(Book book)
        {
            var key = $"ebooks/{book.GetPdfFileName()}";

            using var stream = new MemoryStream(book.PdfBytes);
            return await _s3Service.UploadAsync(stream, key, "application/pdf");
        }

        public string GetPdfDownloadUrl(Book book)
        {
            if (string.IsNullOrEmpty(book.EBookPdfPath))
                return null;

            // URL absoluta (S3): retorna diretamente
            if (book.EBookPdfPath.StartsWith("https://") || book.EBookPdfPath.StartsWith("http://"))
                return book.EBookPdfPath;

            // Storage local: monta URL via backend
            var relativePath = _imageSettings.EBookPdfPath.Replace("wwwroot", "");
            return $"{_serverSettings.BackendUrl}{relativePath}/{book.EBookPdfPath}";
        }

        public void Validate(Book book)
        {
            if (book.Type != BookType.Eletronic)
                return;

            if (!book.HasPdfToUpload() && string.IsNullOrEmpty(book.EBookPdfPath))
            {
                throw new ShareBookException(ShareBookException.Error.BadRequest,
                    "É necessário enviar o arquivo PDF para cadastrar um E-Book.");
            }

            const int maxSizeBytes = 50 * 1024 * 1024; // 50MB
            if (book.HasPdfToUpload() && book.PdfBytes.Length > maxSizeBytes)
            {
                throw new ShareBookException(ShareBookException.Error.BadRequest,
                    "O arquivo PDF não pode ser maior que 50MB.");
            }
        }
    }
}
