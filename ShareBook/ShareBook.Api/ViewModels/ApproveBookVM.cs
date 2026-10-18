using System;

namespace ShareBook.Api.ViewModels;

public class ApproveBookVM
{
    /// <summary>
    /// The date that the winner will be choosed
    /// </summary>
    public DateTime? ChooseDate { get; set; }
    /// <summary>Opcional. Se futura (fuso de São Paulo), o livro físico fica Scheduled até essa data.</summary>
    public DateOnly? ReleaseDate { get; set; }
}
