using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShareBook.Api.ViewModels {
    public class UserCancellationInfoVM {
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        internal UserVM User { get; set; }
        public DateTime CancellationUserDate { get; set; }

        [Required(ErrorMessage = "O campo {0} é muito importante para nós e deve ser preenchido, obrigado!")]
        [StringLength(250, ErrorMessage = "O campo {0} deve ter entre {2} e {1} caracteres.", MinimumLength = 2 )]
        public string Reason { get; set; }
    }
}