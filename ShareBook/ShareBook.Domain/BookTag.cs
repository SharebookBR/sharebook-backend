using ShareBook.Domain.Common;
using ShareBook.Domain.Enums;
using System;

namespace ShareBook.Domain;

public class BookTag : BaseEntity
{
    public Guid BookId { get; set; }

    public Book Book { get; set; } = null!;

    public string TagId { get; set; } = string.Empty;

    public Tag Tag { get; set; } = null!;

    public int Position { get; set; }

    public BookTagSource Source { get; set; } = BookTagSource.Manual;

    public BookTagReviewStatus ReviewStatus { get; set; } = BookTagReviewStatus.Approved;
}
