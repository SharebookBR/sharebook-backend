using FluentValidation;

namespace ShareBook.Domain.Validators
{
    public class MeetupValidator : AbstractValidator<Meetup>
    {
        public MeetupValidator()
        {
            RuleFor(m => m.Title)
                .NotEmpty()
                .WithMessage("A propriedade: {propertyName} é obrigatória!");

            RuleFor(m => m.StartDate)
                .NotEmpty()
                .WithMessage("A propriedade: {propertyName} é obrigatória!");

            RuleFor(m => m.SymplaEventUrl)
                .NotEmpty()
                .WithMessage("A propriedade: {propertyName} é obrigatória!");

            RuleFor(m => m.SymplaEventId)
                .NotEmpty()
                .WithMessage("A propriedade: {propertyName} é obrigatória!");
        }
    }
}
