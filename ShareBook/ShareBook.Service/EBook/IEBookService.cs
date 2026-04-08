using ShareBook.Domain;
using System.Threading.Tasks;

namespace ShareBook.Service.EBook
{
    public interface IEBookService
    {
        /// <summary>
        /// Faz upload do PDF do e-book usando o slug do livro como nome do arquivo
        /// </summary>
        /// <param name="book">Livro com os bytes do PDF</param>
        /// <returns>Caminho do arquivo salvo</returns>
        Task<string> UploadPdfAsync(Book book);

        /// <summary>
        /// Obtém a URL de download do PDF do e-book quando aplicável.
        /// Para S3 privado, retorna URL assinada temporária.
        /// Para storage local, retorna null (download é feito pelo endpoint local).
        /// </summary>
        /// <param name="book">Livro</param>
        /// <returns>URL para download/redirect ou null.</returns>
        Task<string> GetPdfDownloadUrlAsync(Book book);

        /// <summary>
        /// Remove o PDF associado ao e-book, quando existir.
        /// </summary>
        /// <param name="book">Livro</param>
        Task DeletePdfAsync(Book book);

        /// <summary>
        /// Valida se o e-book possui os dados necessários
        /// </summary>
        /// <param name="book">Livro</param>
        void Validate(Book book);
    }
}
