using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ShareBook.Domain;
using ShareBook.Domain.Validators;
using ShareBook.Repository;
using ShareBook.Repository.Infra;
using ShareBook.Repository.Infra.CrossCutting.Identity.Configurations;
using ShareBook.Repository.Infra.CrossCutting.Identity.Interfaces;
using ShareBook.Service;
using ShareBook.Service.Upload;

namespace ShareBook.Api.Configuration
{
    public static class ServiceRepositoryCollectionExtensions
    {
        public static IServiceCollection RegisterRepositoryServices(
           this IServiceCollection services)
        {
            //services
            services.AddScoped<IBooksEmailService, BooksEmailService>();
            services.AddScoped<IBookUsersEmailService, BookUserEmailService>();
            services.AddScoped<IBookService, BookService>();
            services.AddScoped<IBookUserService, BookUserService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ICategoryService, CategoryService>();

            //repositories
            services.AddScoped<IBookRepository, BookRepository>();
            services.AddScoped<IBookUserRepository, BookUserRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();

            //validators
            services.AddScoped<IValidator<User>, UserValidator>();
            services.AddScoped<IValidator<Book>, BookValidator>();
            services.AddScoped<IValidator<Category>, CategoryValidator>();

            //Auth
            services.AddScoped<IApplicationSignInManager, ApplicationSignInManager>();

            //Email 
            services.AddScoped<IEmailService, EmailService>();
            services.AddSingleton<IEmailTemplate, EmailTemplate>();

            //Upload 
            services.AddScoped<IUploadService, UploadService>();

            //UnitOfWork
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
