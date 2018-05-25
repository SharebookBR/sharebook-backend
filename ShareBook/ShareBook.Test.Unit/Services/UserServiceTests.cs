using Moq;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Domain.Validators;
using ShareBook.Repository;
using ShareBook.Repository.Infra;
using ShareBook.Repository.Infra.CrossCutting.Identity.Interfaces;
using ShareBook.Service;
using Xunit;

namespace ShareBook.Test.Unit.Services
{
    public class UserServiceTests
    {
        Mock<IUserService> userServiceMock;
        Mock<IApplicationSignInManager> signManagerMock;
        Mock<IUserRepository> userRepositoryMock;
        Mock<IUnitOfWork> unitOfWorkMock;

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
                    Email = "walter.vlopes@gmail.com",
                    Password = "123456"
                };
            });

            userServiceMock.Setup(service => service.Insert(It.IsAny<User>())).Verifiable();
        }

        [Fact]
        public void RegisterValidUser()
        {
            var service = new UserService(userRepositoryMock.Object, unitOfWorkMock.Object, new UserValidator());
         
            Result<User> result = service.Insert(new User()
            {
                Email = "walter.vlopes@gmail.com",
                Password = "123456"
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
    }
}
