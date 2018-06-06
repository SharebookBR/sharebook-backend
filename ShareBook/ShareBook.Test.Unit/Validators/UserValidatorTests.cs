using FluentValidation.Results;
using ShareBook.Domain;
using ShareBook.Domain.Validators;
using Xunit;

namespace ShareBook.Test.Unit.Validators
{
    public class UserValidatorTests
    {
        UserValidator userValidation = new UserValidator();

        [Fact]
        public void ValidEntities()
        {
			User user = new User()
			{
				Email = "joão@sharebook.com",
				Password = "password",
				Name = "João da Silva",
				Linkedin = "linkedin.com/joao-silva",
				Cep = "04473-190"
            };

            ValidationResult result = userValidation.Validate(user);

            Assert.True(result.IsValid);
        }

		[Fact]
		public void InvalidEntities()
        {
            User user = new User()
            {
                Email = "joão@sharebook.com",
                Password = null,
				Name = null,
				Linkedin = "linkedin.com/joao-silva",
				Cep = "04473-190"
			};

            ValidationResult result = userValidation.Validate(user);

            Assert.False(result.IsValid);
        }
    }
}
