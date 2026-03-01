using Microsoft.Extensions.Options;
using ShareBook.Domain;
using ShareBook.Domain.Enums;
using ShareBook.Domain.Exceptions;
using ShareBook.Service.Upload;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ShareBook.Service.EBook
{
    public class EBookService : IEBookService
    {
        private const string S3EbookPrefix = "ebooks/";
        private readonly ImageSettings _imageSettings;
        private readonly AwsS3Settings _storageSettings;
        private readonly IS3Service _s3Service;

        public EBookService(
            IOptions<ImageSettings> imageSettings,
            IOptions<AwsS3Settings> storageSettings,
            IS3Service s3Service)
        {
            _imageSettings = imageSettings.Value;
            _storageSettings = storageSettings.Value;
            _s3Service = s3Service;
        }

        public async Task<string> UploadPdfAsync(Book book)
        {
            if (!book.HasPdfToUpload())
                return null;

            if (!_storageSettings.IsActive)
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

        public async Task<string> GetPdfDownloadUrlAsync(Book book)
        {
            if (string.IsNullOrEmpty(book.EBookPdfPath))
                return null;

            // Registros antigos podem ter URL absoluta salva no banco.
            if (book.EBookPdfPath.StartsWith("https://") || book.EBookPdfPath.StartsWith("http://"))
                return book.EBookPdfPath;

            // Storage local: endpoint faz stream do arquivo com validação de path.
            if (!_storageSettings.IsActive)
                return null;

            // Storage S3 privado: gera URL assinada temporária.
            var s3Key = NormalizeS3Key(book.EBookPdfPath);
            return await _s3Service.GeneratePreSignedDownloadUrlAsync(s3Key, book.GetPdfFileName());
        }

        private string NormalizeS3Key(string eBookPdfPath)
        {
            var key = eBookPdfPath.Trim();

            // Caminhos antigos seedados sem prefixo precisam apontar para "ebooks/".
            if (!key.Contains("/"))
                return $"{S3EbookPrefix}{key}";

            if (key.StartsWith("/"))
                key = key.TrimStart('/');

            return key;
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
