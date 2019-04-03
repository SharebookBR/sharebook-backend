using System.ComponentModel.DataAnnotations;

namespace ShareBook.Api.ViewModels
{
    public class LoginUserVM
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public string PlayerId { get; set; }
    }
}
