using AutoMapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Domain.Validators;
using ShareBook.Infra.CrossCutting.Identity.Interfaces;
using ShareBook.Repository;
using ShareBook.Repository.UoW;
using ShareBook.Service;
using ShareBook.Service.Authorization;
using ShareBook.Service.Recaptcha;
using ShareBook.Test.Unit.Mocks;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ShareBook.Test.Unit.Services;

public class UserServiceTests
{
    readonly Mock<IUserService> userServiceMock;
    readonly Mock<IApplicationSignInManager> signManagerMock;
    readonly Mock<IUserRepository> userRepositoryMock;
    readonly Mock<IBookRepository> bookRepositoryMock;
    readonly Mock<IUnitOfWork> unitOfWorkMock;
    readonly Mock<IUserEmailService> userEmailServiceMock;
    readonly Mock<IMapper> mapperMock;
    readonly Mock<IRecaptchaService> recaptchaServiceMock;
    readonly Mock<ILogger<UserService>> loggerMock;
    readonly Mock<IConfiguration> configMock;
    readonly Guid _currentUserId;

    public UserServiceTests()
    {
        // Definindo quais serão as classes mockadas
        userServiceMock = new Mock<IUserService>();
        signManagerMock = new Mock<IApplicationSignInManager>();
        unitOfWorkMock = new Mock<IUnitOfWork>();
        userRepositoryMock = new Mock<IUserRepository>();
        bookRepositoryMock = new Mock<IBookRepository>();
        userEmailServiceMock = new Mock<IUserEmailService>();
        mapperMock = new Mock<IMapper>();
        recaptchaServiceMock = new Mock<IRecaptchaService>();
        loggerMock = new Mock<ILogger<UserService>>();
        configMock = new Mock<IConfiguration>();

        //Simula login do usuario
        var claimsUser = new UserMock().GetClaimsUser();
        Thread.CurrentPrincipal = claimsUser;
        _currentUserId = new Guid(claimsUser.Identity.Name);

        userServiceMock.Setup(service => service.InsertAsync(It.IsAny<User>())).Verifiable();
        userServiceMock.Setup(service => service.UpdateAsync(It.IsAny<User>())).Verifiable();
    }

    private static ApplicationDbContext CreateContext(string databaseName = null) => new ApplicationDbContext(
        new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options);

    private UserService CreateService(ApplicationDbContext context)
        => new UserService(userRepositoryMock.Object, bookRepositoryMock.Object, context, unitOfWorkMock.Object,
            new UserValidator(), mapperMock.Object, userEmailServiceMock.Object, recaptchaServiceMock.Object, configMock.Object);

    #region Register User

    [Fact]
    public async Task RegisterInvalidUser()
    {
        await using var context = CreateContext();
        var service = CreateService(context);

        Result<User> result = await service.InsertAsync(new User()
        {
            Email = "",
            Password = ""
        });
        Assert.NotNull(result);
        Assert.False(result.Success);
    }
    #endregion

    #region Update User

    [Fact]
    public async Task UpdateValidUser()
    {
        // ChangeAddress() troca a instância de Address por uma nova (reaproveitando o Id
        // antigo) e o UpdateAsync depende do EF fazer o fixup dessa troca via chave —
        // o provider InMemory não resolve esse padrão da mesma forma que um provider
        // relacional de verdade (Postgres em produção), por isso usamos SQLite aqui.
        await using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options;
        await using var context = new ApplicationDbContext(options);
        await context.Database.EnsureCreatedAsync();

        context.Users.Add(new User
        {
            Id = _currentUserId,
            Name = "Sergio (antes do update)",
            Email = "sergio.antigo@example.com",
            Password = "x",
            PasswordSalt = "y",
            Address = new Address
            {
                PostalCode = "00000-000",
                Street = "Rua antiga",
                Number = "1",
                City = "São Paulo",
                Country = "Brasil",
                State = "SP",
                Neighborhood = "Antigo"
            }
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        Result<User> result = await service.UpdateAsync(new User()
        {
            Email = "sergioprates.student@gmail.com",
            Linkedin = "https://www.linkedin.com/in/sergiopratesdossantos/",
            Name = "Sergio1",
            Phone = "584558999",
            Address = new Address()
            {
                PostalCode = "04473-140",
                Street = "Av sharebook",
                Number = "5",
                City = "São Paulo",
                Country = "Brasil",
                State = "SP",
                Neighborhood = "Interlagos"
            }
        });

        Assert.NotNull(result);
        Assert.True(result.Success);
    }

    [Fact]
    public async Task UpdateInvalidUser()
    {
        await using var context = CreateContext();
        var service = CreateService(context);

        Result<User> result = await service.UpdateAsync(new User()
        {
            Email = "",
            Linkedin = "",
            Name = "",
            Phone = "",
        });

        Assert.NotNull(result);
        Assert.False(result.Success);
    }

    [Fact]
    public async Task UpdateUserNotExists()
    {
        await using var context = CreateContext();
        var service = CreateService(context);

        Result<User> result = await service.UpdateAsync(new User()
        {
            Email = "sss@sss.com",
            Linkedin = ""
        });

        Assert.NotNull(result);
        Assert.False(result.Success);
    }

    #endregion

    #region Login User
    [Fact]
    public async Task LoginValidUser()
    {
        await using var context = CreateContext();
        context.Users.Add(UserMock.GetGrantee());
        await context.SaveChangesAsync();

        var service = CreateService(context);
        Result<User> result = await service.AuthenticationByEmailAndPasswordAsync(new User()
        {
            Email = "walter@sharebook.com",
            Password = "123456"
        });
        Assert.NotNull(result);
        Assert.Empty(result.Value.Password);
        Assert.Empty(result.Value.PasswordSalt);
        Assert.NotEmpty(result.Value.Name);
        Assert.True(result.Success);
    }

    [Fact]
    public async Task LoginInvalidPassword()
    {
        await using var context = CreateContext();
        context.Users.Add(UserMock.GetGrantee());
        await context.SaveChangesAsync();

        var service = CreateService(context);
        Result<User> result = await service.AuthenticationByEmailAndPasswordAsync(new User()
        {
            Email = "walter@sharebook.com",
            Password = "wrongpassword"
        });
        Assert.Equal("Email ou senha incorretos", result.Messages[0]);
        Assert.False(result.Success);
    }

    [Fact]
    public async Task LoginInvalidEmail()
    {
        await using var context = CreateContext();
        context.Users.Add(UserMock.GetGrantee());
        await context.SaveChangesAsync();

        var service = CreateService(context);
        Result<User> result = await service.AuthenticationByEmailAndPasswordAsync(new User()
        {
            Email = "joao@sharebook.com",
            Password = "wrongpassword"
        });
        Assert.Equal("Não encontramos esse email no Sharebook. Você já se cadastrou?", result.Messages[0]);
        Assert.False(result.Success);
    }
    #endregion
}
