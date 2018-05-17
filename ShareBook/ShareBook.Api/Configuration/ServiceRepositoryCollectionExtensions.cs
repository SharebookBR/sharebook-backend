using Microsoft.Extensions.DependencyInjection;
using ShareBook.Repository;
using ShareBook.Repository.Infra;
using ShareBook.Service;

namespace ShareBook.Api.Configuration
{
    public static class ServiceRepositoryCollectionExtensions
    {
        public static IServiceCollection RegisterRepositoryServices(
           this IServiceCollection services)
        {
            //services
            services.AddTransient<IBookService, BookService>();
            services.AddTransient<IUserService, UserService>();

            //repositories
            services.AddTransient<IBookRepository, BookRepository>();
            services.AddTransient<IUserRepository, UserRepository>();

            //UnitOfWork
            services.AddTransient<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
