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

        public EBookService(IOptions<ImageSettings> imageSettings, IOptions<ServerSettings> serverSettings)
        {
            _imageSettings = imageSettings.Value;
            _serverSettings = serverSettings.Value;
        }

        public async Task<string> UploadPdfAsync(Book book)
        {
            if (!book.HasPdfToUpload())
                return null;

            var pdfFileName = book.GetPdfFileName();
            // Usa o caminho configurado no appsettings (ex: wwwroot/EbookPdfs)
            var fullDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _imageSettings.EBookPdfPath);

            if (!Directory.Exists(fullDirectoryPath))
                Directory.CreateDirectory(fullDirectoryPath);

            var pdfFullPath = Path.Combine(fullDirectoryPath, pdfFileName);
            await File.WriteAllBytesAsync(pdfFullPath, book.PdfBytes);

            // Retorna apenas o nome do arquivo (o caminho do diretório já é conhecido via config)
            return pdfFileName;
        }

        public string GetPdfDownloadUrl(Book book)
        {
            if (string.IsNullOrEmpty(book.EBookPdfPath))
                return null;

            // Monta a URL completa para download
            var relativePath = _imageSettings.EBookPdfPath.Replace("wwwroot", "");
            return $"{_serverSettings.BackendUrl}{relativePath}/{book.EBookPdfPath}";
        }

        public void Validate(Book book)
        {
            if (book.Type != BookType.Eletronic)
                return;

            // E-book deve ter PDF para upload
            if (!book.HasPdfToUpload() && string.IsNullOrEmpty(book.EBookPdfPath))
            {
                throw new ShareBookException(ShareBookException.Error.BadRequest,
                    "E necessario enviar o arquivo PDF para cadastrar um E-Book.");
            }
        }
    }
}