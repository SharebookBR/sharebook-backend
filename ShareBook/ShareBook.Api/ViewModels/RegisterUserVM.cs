
using ShareBook.Api.ViewModels.CustomValidators;
using System.ComponentModel.DataAnnotations;

namespace ShareBook.Api.ViewModels
{
    public class RegisterUserVM
    {
        [Required(ErrorMessage = "Nome é obrigatório")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Rua é obrigatório")]
        public string Street { get; set; }

        [Required(ErrorMessage = "Número é obrigatório")]
        public string Number { get; set; }

        [StringLengthRangeOptional(ErrorMessage = "Complemento deve ter no máximo 50 caracteres", Minimum = 0, Maximum = 50)]
        public string Complement { get; set; }

        [Required(ErrorMessage = "Bairro é obrigatorio")]
        public string Neighborhood { get; set; }

        [Required(ErrorMessage = "CEP é obrigatorio")]
        public string PostalCode { get; set; }

        [Required(ErrorMessage = "Cidade é obrigatorio")]
        public string City { get; set; }

        [Required(ErrorMessage = "Estado é obrigatorio")]
        public string State { get; set; }

        [Required(ErrorMessage = "País é obrigatorio")]
        public string Country { get; set; }

        [StringLengthRangeOptional(ErrorMessage = "Linkedin deve ter no máximo 100 caracteres", Minimum = 0, Maximum = 100)]
        public string Linkedin { get; set; }

        [StringLengthRangeOptional(ErrorMessage = "Telefone deve ter no máximo 100 caracteres", Minimum = 0, Maximum = 100)]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Senha é obrigatória")]
        public string Password { get; set; }

        public bool AllowSendingEmail { get; set; } = true;
    }
}
