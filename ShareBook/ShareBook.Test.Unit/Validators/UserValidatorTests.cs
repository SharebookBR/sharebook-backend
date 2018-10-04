using FluentValidation.Results;
using ShareBook.Domain.Entities;
using ShareBook.Domain.Validators;
using Xunit;

namespace ShareBook.Test.Unit.Validators
{
    public class UserValidatorTests
    {
        UserValidator userValidation;
        User userPasswordTest;

        public UserValidatorTests()
        {
            userValidation = new UserValidator();
            userPasswordTest = new User();

        }

        [Fact]
        public void ValidEntities()
        {
			User user = new User()
			{
				Email = "joão@sharebook.com",
				Password = "Password.123",
				Name = "João da Silva",
				Linkedin = "linkedin.com/joao-silva",
                PostalCode = "04473-190"
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
                PostalCode = "04473-190"
			};

            ValidationResult result = userValidation.Validate(user);

            Assert.False(result.IsValid);
        }

        [Fact]
        public void PasswordOnlyNumbers()
        {
            userPasswordTest.Password = "123456";

            var result = userPasswordTest.PasswordIsStrong();

            Assert.False(result);
        }

        [Fact]
        public void PasswordOnlyLetters()
        {
            userPasswordTest.Password = "password";

            var result = userPasswordTest.PasswordIsStrong();

            Assert.False(result);
        }

        [Fact]
        public void PasswordLettersNumbers()
        {
            userPasswordTest.Password = "password123";

            var result = userPasswordTest.PasswordIsStrong();

            Assert.False(result);
        }

        [Fact]
        public void PasswordSpecialCharacter()
        {
            userPasswordTest.Password = "password.123";

            var result = userPasswordTest.PasswordIsStrong();

            Assert.False(result);
        }

        [Fact]
        public void PasswordValid()
        {
            userPasswordTest.Password = "QweRty@123!";

            var result = userPasswordTest.PasswordIsStrong();

            Assert.True(result);
        }

        [Fact]
        public void PasswordTwoValid()
        {
            userPasswordTest.Password = "601jFy0IN#";

            var result = userPasswordTest.PasswordIsStrong();

            Assert.True(result);
        }

        [Fact]
        public void PasswordThreeValid()
        {
            userPasswordTest.Password = "Anu-P2017";

            var result = userPasswordTest.PasswordIsStrong();

            Assert.True(result);
        }
    }
}
