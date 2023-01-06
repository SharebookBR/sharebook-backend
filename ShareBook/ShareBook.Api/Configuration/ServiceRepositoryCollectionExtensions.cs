using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Sharebook.Jobs;
using ShareBook.Domain;
using ShareBook.Domain.Validators;
using ShareBook.Infra.CrossCutting.Identity;
using ShareBook.Infra.CrossCutting.Identity.Interfaces;
using ShareBook.Repository;
using ShareBook.Repository.UoW;
using ShareBook.Service;
using ShareBook.Service.AwsSqs;
using ShareBook.Service.Lgpd;
using ShareBook.Service.Muambator;
using ShareBook.Service.Notification;
using ShareBook.Service.Recaptcha;
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
            services.AddScoped<IUserEmailService, UserEmailService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IContactUsService, ContactUsService>();
            services.AddScoped<IContactUsEmailService, ContactUsEmailService>();
            services.AddScoped<IMuambatorService, MuambatorService>();
            services.AddScoped<IAccessHistoryService, AccessHistoryService>();
            services.AddScoped<ILgpdService, LgpdService>();
            services.AddScoped<IMeetupService, MeetupService>();
            services.AddScoped<IRecaptchaService, RecaptchaService>();

            //repositories
            services.AddScoped<IBookRepository, BookRepository>();
            services.AddScoped<IBookUserRepository, BookUserRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IJobHistoryRepository, JobHistoryRepository>();
            services.AddScoped<IAccessHistoryRepository, AccessHistoryRepository>();
            services.AddScoped<IMeetupRepository, MeetupRepository>();

            //validators
            services.AddScoped<IValidator<User>, UserValidator>();
            services.AddScoped<IValidator<Book>, BookValidator>();
            services.AddScoped<IValidator<Category>, CategoryValidator>();
            services.AddScoped<IValidator<ContactUs>, ContactUsValidator>();
            services.AddScoped<IValidator<BookUser>, BookUserValidator>();
            services.AddScoped<IValidator<Address>, AddressValidator>();
            services.AddScoped<IValidator<AccessHistory>, AccessHistoryValidator>();
            services.AddScoped<IValidator<Meetup>, MeetupValidator>();

            //Auth
            services.AddScoped<IApplicationSignInManager, ApplicationSignInManager>();

            //Email
            services.AddScoped<IEmailService, EmailService>();
            services.AddSingleton<IEmailTemplate, EmailTemplate>();

            //Upload
            services.AddScoped<IUploadService, UploadService>();

            //UnitOfWork
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            //Queues
            services.AddScoped<NewBookQueue>();
            services.AddScoped<MailSenderLowPriorityQueue>();
            services.AddScoped<MailSenderHighPriorityQueue>();
            // TODO: colocar as outras duas queues

            //Jobs
            services.AddScoped<IJobExecutor, JobExecutor>();
            services.AddScoped<CancelAbandonedDonations>();
            services.AddScoped<RemoveBookFromShowcase>();
            services.AddScoped<ChooseDateReminder>();
            services.AddScoped<LateDonationNotification>();
            services.AddScoped<MeetupSearch>();
            services.AddScoped<NewBookGetInterestedUsers>();
            services.AddScoped<MailSupressListUpdate>();
            services.AddScoped<MailSender>();

            //notification
            services.AddScoped<IPushNotificationService, PushNotificationService>();

            return services;
        }
    }
}