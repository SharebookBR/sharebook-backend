using ShareBook.Domain.Common;

namespace ShareBook.Domain
{
    public class Book : BaseEntity
    {
        public string Name { get; set; }
        public bool Approved { get; set; } = false;
    }
}
