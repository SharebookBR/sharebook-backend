using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShareBook.Domain.Validators
{
    public class CategoryValidator : AbstractValidator<Category>
    {

        #region Messages
        public const string Name = "O nome da categoria é obrigatório";
        #endregion


        public CategoryValidator()
        {
            RuleFor(c => c.Name)
                .NotEmpty()
                .WithMessage(Name);
        }
    }
}
