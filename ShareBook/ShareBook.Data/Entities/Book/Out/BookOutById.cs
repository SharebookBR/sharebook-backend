using ShareBook.Data.Common;
using ShareBook.Data.Entities.Book.Model;
namespace ShareBook.Data.Entities.Book.Out
{
    public class BookOutById : ResultService
    {
        public BookModel Book { get; set; }
    }
}
