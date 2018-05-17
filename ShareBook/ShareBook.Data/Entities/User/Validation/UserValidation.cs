using FluentValidation;
using ShareBook.Data.Entities.User;
using ShareBook.Data.Model;

namespace ShareBook.Data
{
    public class UserValidation : AbstractValidator<User>
    {
        public UserValidation()
        {
            RuleFor(u => u.Email)
               .NotEmpty().WithMessage(UserMessage.Validation.Email);

            RuleFor(u => u.Password)
              .NotEmpty().WithMessage(UserMessage.Validation.Password);
        }
    }
}
