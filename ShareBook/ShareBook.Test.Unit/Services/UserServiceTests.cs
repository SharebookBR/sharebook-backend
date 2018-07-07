using Moq;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Domain.Validators;
using ShareBook.Repository;
using ShareBook.Repository.Infra;
using ShareBook.Repository.Infra.CrossCutting.Identity.Interfaces;
using ShareBook.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ShareBook.Test.Unit.Services
{
    public class UserServiceTests
    {
        readonly Mock<IUserService> userServiceMock;
        readonly Mock<IApplicationSignInManager> signManagerMock;
        readonly Mock<IUserRepository> userRepositoryMock;
        readonly Mock<IUnitOfWork> unitOfWorkMock;

        private const string PASSWORD_HASH = "9XurTqQsYQY1rtAGXRfwEWO/ROghN3DFx9lTT75i/0s=";
        private const string PASSWORD_SALT = "1x7XxoaSO5I0QGIdARCh5A==";

        public UserServiceTests()
        {
            // Definindo quais serão as classes mockadas
            userServiceMock = new Mock<IUserService>();
            signManagerMock = new Mock<IApplicationSignInManager>();
            unitOfWorkMock = new Mock<IUnitOfWork>();
            userRepositoryMock = new Mock<IUserRepository>();

            userRepositoryMock.Setup(repo => repo.Insert(It.IsAny<User>())).Returns(() =>
            {
                return new User()
                {
                    Email = "jose@sharebook.com",
                    Password = "123456",
                    Name = "José da Silva",
                    Linkedin = "linkedin.com/jose",
                    PostalCode = "04473-190"
                };
            });

            userRepositoryMock.Setup(repo => repo.Get()).Returns(() =>
            {
                return new List<User>()
                {
                    new User()
                        {
                            Id = Guid.NewGuid(),
                            Email = "jose@sharebook.com",
                            Password = PASSWORD_HASH,
                            PasswordSalt = PASSWORD_SALT,
                             Name = "José da Silva",
                            Linkedin = "linkedin.com/jose",
                            PostalCode = "04473-190"
                        }
                }.AsQueryable();
            });

            userServiceMock.Setup(service => service.Insert(It.IsAny<User>())).Verifiable();
        }

        #region Register User
        [Fact]
        public void RegisterValidUser()
        {
            var service = new UserService(userRepositoryMock.Object, unitOfWorkMock.Object, new UserValidator());

            Result<User> result = service.Insert(new User()
            {
                Email = "jose@sharebook.com",
                Password = "123456",
				Name = "José da Silva",
				Linkedin = @"linkedin.com\jose-silva",
                PostalCode = "04473-190"

            });
            Assert.NotNull(result);
            Assert.True(result.Success);
        }

        [Fact]
        public void RegisterInvalidUser()
        {
            var service = new UserService(userRepositoryMock.Object, unitOfWorkMock.Object, new UserValidator());

            Result<User> result = service.Insert(new User()
            {
                Email = "",
                Password = ""
            });
            Assert.NotNull(result);
            Assert.False(result.Success);
        }
        #endregion


        #region Login User
        [Fact]
        public void LoginValidUser()
        {
            var service = new UserService(userRepositoryMock.Object, unitOfWorkMock.Object, new UserValidator());
            Result<User> result = service.AuthenticationByEmailAndPassword(new User()
            {
                Email = "jose@sharebook.com",
                Password = "123456"
            });
            Assert.NotNull(result);
            Assert.Empty(result.Value.Password);
            Assert.Empty(result.Value.PasswordSalt);
            Assert.NotEmpty(result.Value.Name);
            Assert.True(result.Success);
        }

        [Fact]
        public void LoginInvalidPassword()
        {
            var service = new UserService(userRepositoryMock.Object, unitOfWorkMock.Object, new UserValidator());
            Result<User> result = service.AuthenticationByEmailAndPassword(new User()
            {
                Email = "jose@sharebook.com",
                Password = "wrongpassword"
            });
            Assert.Equal("Email ou senha incorretos", result.Messages[0]);
            Assert.False(result.Success);
        }

        [Fact]
        public void LoginInvalidEmail()
        {
            var service = new UserService(userRepositoryMock.Object, unitOfWorkMock.Object, new UserValidator());
            Result<User> result = service.AuthenticationByEmailAndPassword(new User()
            {
                Email = "joao@sharebook.com",
                Password = "wrongpassword"
            });
            Assert.Equal("Email ou senha incorretos", result.Messages[0]);
            Assert.False(result.Success);
        } 
        #endregion
    }
}
