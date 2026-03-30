using ShareBook.Service;
using Xunit;

namespace ShareBook.Test.Unit.Services
{
    public class EmailAddressValidatorTests
    {
        [Theory]
        [InlineData("joao@example.com")]
        [InlineData("maria.silva+tag@gmail.com")]
        public void IsValid_ReturnsTrue_ForValidEmails(string email)
        {
            Assert.True(EmailAddressValidator.IsValid(email));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("amantedoslivrosfisicos09.@gmail.com")]
        [InlineData("lokapandinha10@gmail..com")]
        [InlineData("sem-arroba.com")]
        public void IsValid_ReturnsFalse_ForInvalidEmails(string email)
        {
            Assert.False(EmailAddressValidator.IsValid(email));
        }
    }
}
