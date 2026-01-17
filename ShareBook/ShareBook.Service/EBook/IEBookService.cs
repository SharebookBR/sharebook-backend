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
        /// Obtém a URL de download do PDF do e-book
        /// </summary>
        /// <param name="book">Livro</param>
        /// <returns>URL para download</returns>
        string GetPdfDownloadUrl(Book book);

        /// <summary>
        /// Valida se o e-book possui os dados necessários
        /// </summary>
        /// <param name="book">Livro</param>
        void Validate(Book book);
    }
}