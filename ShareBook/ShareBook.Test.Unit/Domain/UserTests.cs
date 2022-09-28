using ShareBook.Domain;
using ShareBook.Helper.Crypto;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ShareBook.Test.Unit.Domain
{
    public class UserTests
    {
        [Fact]
        public void HashCodePasswordDateExpired()
        {
            var expectedDay = DateTime.Now.AddDays(2);
            var hashCodePassword = Salt.Create();
            var user = new User()
            {
                HashCodePassword = hashCodePassword,
                HashCodePasswordExpiryDate = expectedDay
            };

           Assert.False(user.HashCodePasswordIsValid(hashCodePassword));
        }

        [Fact]
        public void HashCodePasswordDateValid()
        {
            var expectedDay = DateTime.Now.AddDays(1);
            var hashCodePassword = Salt.Create();
            var user = new User()
            {
                HashCodePassword = hashCodePassword,
                HashCodePasswordExpiryDate = expectedDay
            };

            Assert.True(user.HashCodePasswordIsValid(hashCodePassword));
        }

        [Fact]
        public void HashCodeInValid()
        {
            var expectedDay = DateTime.Now.AddDays(1);
            var hashCodePassword = Salt.Create();
            var user = new User()
            {
                HashCodePassword = hashCodePassword,
                HashCodePasswordExpiryDate = expectedDay
            };

            Assert.False(user.HashCodePasswordIsValid("HASHCODEINVALID"));
        }

        [Fact]
        public void GenerateHashCodePasswordValid()
        {
            var expectedDay = DateTime.Now.AddDays(1);
            var hashCodePassword = Salt.Create();
            var user = new User()
            {
                HashCodePassword = hashCodePassword,
                HashCodePasswordExpiryDate = expectedDay
            };

            Assert.Equal(user.HashCodePasswordExpiryDate.Date, expectedDay.Date);
        }

        [Fact]
        public void CheckBruteForceTrue()
        {
            var user = new User() { LastLogin = DateTime.Now.AddSeconds(-5) };
            Assert.True(user.IsBruteForceLogin());
        }

        [Fact]
        public void CheckBruteForceFalse()
        {
            var user = new User() { LastLogin = DateTime.Now.AddSeconds(-31) };
            Assert.False(user.IsBruteForceLogin());
        }
    }
}
