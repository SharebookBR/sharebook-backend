using ShareBook.Data.Common;
using ShareBook.Data.Entities.Book.Model;
using System.Collections.Generic;

namespace ShareBook.Data.Entities.Book.Out
{
    public class BookOut : ResultService
    {
        public BookOut()
        {
            Books = new List<BookModel>();
        }

        public List<BookModel> Books { get; set; }
    }
}
