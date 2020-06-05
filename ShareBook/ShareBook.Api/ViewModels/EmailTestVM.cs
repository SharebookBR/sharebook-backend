using System.ComponentModel.DataAnnotations;

namespace ShareBook.Api.ViewModels
{
    public class EmailTestVM
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
