using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ShareBook.Domain;
using ShareBook.Domain.Validators;
using ShareBook.Infra.CrossCutting.Identity;
using ShareBook.Infra.CrossCutting.Identity.Interfaces;
using ShareBook.Repository;
using ShareBook.Repository.UoW;
using ShareBook.Service;
using ShareBook.Service.Upload;
using Sharebook.Jobs;
using ShareBook.Service.Notification;
using ShareBook.Service.Muambator;

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
            services.AddScoped<IUserEmailService, UserEmailService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IContactUsService, ContactUsService>();
            services.AddScoped<IContactUsEmailService, ContactUsEmailService>();
            services.AddScoped<IMuambatorService, MuambatorService>();

            //repositories
            services.AddScoped<IBookRepository, BookRepository>();
            services.AddScoped<IBookUserRepository, BookUserRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IJobHistoryRepository, JobHistoryRepository>();

            //validators
            services.AddScoped<IValidator<User>, UserValidator>();
            services.AddScoped<IValidator<Book>, BookValidator>();
            services.AddScoped<IValidator<Category>, CategoryValidator>();
            services.AddScoped<IValidator<ContactUs>, ContactUsValidator>();
            services.AddScoped<IValidator<BookUser>, BookUserValidator>();

            //Auth
            services.AddScoped<IApplicationSignInManager, ApplicationSignInManager>();

            //Email 
            services.AddScoped<IEmailService, EmailService>();
            services.AddSingleton<IEmailTemplate, EmailTemplate>();

            //Upload 
            services.AddScoped<IUploadService, UploadService>();

            //UnitOfWork
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            //Jobs
            services.AddScoped<IJobExecutor, JobExecutor>();
            services.AddScoped<RemoveBookFromShowcase>();
            services.AddScoped<ChooseDateReminder>();
            services.AddScoped<LateDonationNotification>();

            //notification
            services.AddScoped<INotificationService, NotificationService>();

            return services;
        }
    }
}
