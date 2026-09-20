using ShareBook.Domain.Enums;
using System;

namespace ShareBook.Api.ViewModels;

public class CreateBookVM : BaseViewModel
{
    public required string Title { get; set; }

    public required string Author { get; set; }

    public Guid CategoryId { get; set; }

    // Obrigatório no insert (BookValidator.ImageName.NotEmpty()).
    public required string ImageName { get; set; }

    // Obrigatório no insert (BookValidator.ImageBytes.NotEmpty()).
    public required byte[] ImageBytes { get; set; }

    public FreightOption? FreightOption { get; set; }

    public string? Synopsis { get; set; }

    public string Type { get; set; } = "Printed";

    // Obrigatório apenas para e-books (validado em EBookService.Validate).
    public byte[]? PdfBytes { get; set; }
}
