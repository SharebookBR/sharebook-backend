using System.IO;
using System.Threading.Tasks;

namespace ShareBook.Service.EBook
{
    public interface IS3Service
    {
        /// <summary>
        /// Faz upload de um stream para o S3 e retorna a chave do objeto salvo.
        /// </summary>
        /// <param name="content">Stream com o conteúdo do arquivo.</param>
        /// <param name="key">Chave (caminho) do objeto no bucket, ex: "ebooks/meu-livro.pdf"</param>
        /// <param name="contentType">MIME type, ex: "application/pdf"</param>
        /// <returns>Chave do objeto no S3.</returns>
        Task<string> UploadAsync(Stream content, string key, string contentType);

        /// <summary>
        /// Gera uma URL assinada temporária para download de um objeto privado no S3.
        /// </summary>
        /// <param name="key">Chave (caminho) do objeto no bucket.</param>
        /// <param name="fileName">Nome sugerido para download no navegador.</param>
        /// <returns>URL assinada temporária.</returns>
        Task<string> GeneratePreSignedDownloadUrlAsync(string key, string fileName);

        /// <summary>
        /// Remove um objeto do bucket S3.
        /// </summary>
        /// <param name="key">Chave (caminho) do objeto no bucket.</param>
        Task DeleteAsync(string key);
    }
}
