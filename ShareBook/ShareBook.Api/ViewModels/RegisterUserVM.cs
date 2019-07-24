

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
        public string Street { get; set; }

        [Required]
        public string Number { get; set; }

        public string Complement { get; set; }

        [Required]
        public string Neighborhood { get; set; }

        [Required]
        public string PostalCode { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string State { get; set; }

        [Required]
        public string Country { get; set; }

        public string Linkedin { get; set; }

        public string Phone { get; set; }

        [Required]
        public string Password { get; set; }

        public bool AllowSendingEmail { get; set; } = true;
    }
}
