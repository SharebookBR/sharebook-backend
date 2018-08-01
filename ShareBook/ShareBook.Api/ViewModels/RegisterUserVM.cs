

using System.ComponentModel.DataAnnotations;

namespace ShareBook.Api.ViewModels
{
    public class RegisterUserVM
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string PostalCode { get; set; }

        public string Linkedin { get; set; }

        public string Phone { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
