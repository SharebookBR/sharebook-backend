using ShareBook.Domain;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace ShareBook.Service;

// Trechos de texto compartilhados entre e-mails montados fora dos templates.
public static class EmailText
{
    public static string Requests(int count) =>
        count == 1 ? "1 solicitação" : $"{count} solicitações";

    public static string BookRequestsListHtml(IEnumerable<Book> books) =>
        "<ul>" + string.Concat(books.Select(b => $"<li><strong>{WebUtility.HtmlEncode(b.Title)}</strong>: {Requests(b.TotalInterested())}</li>")) + "</ul>";
}
