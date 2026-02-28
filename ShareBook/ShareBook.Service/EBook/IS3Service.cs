using System.IO;
using System.Threading.Tasks;

namespace ShareBook.Service.EBook
{
    public interface IS3Service
    {
        /// <summary>
        /// Faz upload de um stream para o S3 e retorna a URL pública do objeto.
        /// </summary>
        /// <param name="content">Stream com o conteúdo do arquivo.</param>
        /// <param name="key">Chave (caminho) do objeto no bucket, ex: "ebooks/meu-livro.pdf"</param>
        /// <param name="contentType">MIME type, ex: "application/pdf"</param>
        /// <returns>URL pública do objeto no S3.</returns>
        Task<string> UploadAsync(Stream content, string key, string contentType);
    }
}
