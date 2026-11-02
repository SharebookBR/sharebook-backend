using System.ComponentModel.DataAnnotations;

namespace ShareBook.Api.ViewModels;

public class RejectBookVM
{
    [Required(ErrorMessage = "Informe a mensagem para a pessoa doadora.")]
    [StringLength(2000, ErrorMessage = "A mensagem pode ter no máximo 2000 caracteres.")]
    public string Reason { get; set; } = string.Empty;
}
