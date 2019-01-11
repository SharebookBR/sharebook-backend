using System.ComponentModel.DataAnnotations;

namespace ShareBook.Api.ViewModels
{
    public class ForgotMyPasswordVM
    {
        [Required]
        public string Email { get; set; }
    }
}
