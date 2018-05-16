using ShareBook.VM.Book.Model;
using ShareBook.VM.Common;
using System.Collections.Generic;

namespace ShareBook.VM.Book.Out
{
    public class BookOutVM : ResultServiceVM
    {
        public BookOutVM()
        {
            Books = new List<BookVM>();
        }

        public List<BookVM> Books { get; set; }
    }
}
