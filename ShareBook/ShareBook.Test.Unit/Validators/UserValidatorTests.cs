using FluentValidation.Results;
using ShareBook.Data;
using ShareBook.Data.Entities.User;
using ShareBook.VM.User.Model;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ShareBook.Test.Unit.Validators
{
    public class UserValidatorTests
    {
        UserValidation userValidation = new UserValidation();

        [Theory]
        [InlineData("lionel.messi@gmail.com",
            "barcelona")]
        public void ValidEntities(string email, string password)
        {
            User user = new User()
            {
               Email = email,
               Password = password
            };

            ValidationResult result = userValidation.Validate(user);

            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData("emailinvalido",
           "")]
        public void InvalidEntities(string email, string password)
        {
            User user = new User()
            {
                Email = email,
                Password = password
            };

            ValidationResult result = userValidation.Validate(user);

            Assert.False(result.IsValid);
        }
    }
}
