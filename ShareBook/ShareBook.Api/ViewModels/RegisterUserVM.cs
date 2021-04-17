
using ShareBook.Api.ViewModels.CustomValidators;
using System.ComponentModel.DataAnnotations;

namespace ShareBook.Api.ViewModels
{
    public class RegisterUserVM
    {
        [Required(ErrorMessage = "Nome é obrigatório")]
        [MaxLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [MaxLength(100, ErrorMessage = "Email deve ter no máximo 100 caracteres")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Rua é obrigatório")]
        [MaxLength(50, ErrorMessage = "Cidade deve ter no máximo 50 caracteres")]
        public string Street { get; set; }

        [Required(ErrorMessage = "Número é obrigatório")]
        [MaxLength(10, ErrorMessage = "Número deve ter no máximo 10 caracteres")]
        public string Number { get; set; }

        [StringLengthRangeOptional(ErrorMessage = "Complemento deve ter no máximo 50 caracteres", Minimum = 0, Maximum = 50)]
        public string Complement { get; set; }

        [Required(ErrorMessage = "Bairro é obrigatorio")]
        [MaxLength(50, ErrorMessage = "Bairro deve ter no máximo 50 caracteres")]
        public string Neighborhood { get; set; }

        [Required(ErrorMessage = "CEP é obrigatorio")]
        [MaxLength(15, ErrorMessage = "CEP deve ter no máximo 15 caracteres")]
        public string PostalCode { get; set; }

        [Required(ErrorMessage = "Cidade é obrigatorio")]
        [MaxLength(50, ErrorMessage = "Cidade deve ter no máximo 50 caracteres")]
        public string City { get; set; }

        [Required(ErrorMessage = "Estado é obrigatorio")]
        [MaxLength(30, ErrorMessage = "Estado deve ter no máximo 30 caracteres")]
        public string State { get; set; }

        [Required(ErrorMessage = "País é obrigatorio")]
        [MaxLength(50, ErrorMessage = "País deve ter no máximo 50 caracteres")]
        public string Country { get; set; }

        [StringLengthRangeOptional(ErrorMessage = "Linkedin deve ter no máximo 100 caracteres", Minimum = 0, Maximum = 100)]
        public string Linkedin { get; set; }

        [Required(ErrorMessage = "Telefone é obrigatorio")]
        [MaxLength(100, ErrorMessage = "Telefone deve ter no máximo 100 caracteres")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Senha é obrigatória")]
        public string Password { get; set; }

        public bool AllowSendingEmail { get; set; } = true;
    }
}
