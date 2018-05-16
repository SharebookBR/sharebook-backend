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

            //repositories
            services.AddTransient<IBookRepository, BookRepository>();

            //UnitOfWork
            services.AddTransient<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
