using ShareBook.Domain.Enums;
using System;

namespace ShareBook.Api.ViewModels;

public class UpdateBookVM : BaseViewModel
{
    public required string Title { get; set; }

    public required string Author { get; set; }

    public Guid CategoryId { get; set; }

    public Guid UserId { get; set; }

    public bool Approved { get; set; }

    // Imagem é opcional no update (BookService.UpdateAsync mantém a capa existente quando não enviada).
    public string? ImageName { get; set; }

    public byte[]? ImageBytes { get; set; }

    public string? Synopsis { get; set; }

    public DateTime? ChooseDate { get; set; }

    public FreightOption? FreightOption { get; set; }

    public string Type { get; set; } = "Printed";

    public int? MaxRequestsOverride { get; set; }

    public byte[]? PdfBytes { get; set; }
}
