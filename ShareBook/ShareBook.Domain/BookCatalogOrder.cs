using ShareBook.Domain.Enums;
using System.Linq;

namespace ShareBook.Domain;

/// <summary>
/// Ordem de apresentação das listagens públicas que misturam físicos e digitais (categoria, tag e busca):
/// físicos disponíveis, depois físicos "Em breve" (Scheduled, da liberação mais próxima para a mais distante),
/// depois digitais. Dentro de cada grupo, o critério de cada listagem (data de cadastro, relevância...).
/// Decisão do Raffa (2026-10-18): a prévia é a vitrine da campanha e o físico é o diferencial do Sharebook.
/// Se mudar aqui, a busca tem a mesma expressão escrita à mão (BookRepository.FullTextSearch): manter igual.
/// </summary>
public static class BookCatalogOrder
{
    public static IOrderedQueryable<Book> OrderByCatalogGroup(this IQueryable<Book> query)
        => query
            .OrderBy(book => book.Type == BookType.Printed && book.Status == BookStatus.Available
                ? 0
                : book.Status == BookStatus.Scheduled ? 1 : 2)
            .ThenBy(book => book.ReleaseDate);
}
