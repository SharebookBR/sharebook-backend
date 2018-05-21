using Microsoft.EntityFrameworkCore;
using ShareBook.Data.Entities.Book;
using ShareBook.Data.Entities.User;
using ShareBook.Data.Mapping;

namespace ShareBook.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public ApplicationDbContext()
        {

        }

        public DbSet<Book> Books { get; set; }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            new BookMap(modelBuilder.Entity<Book>());

            new UserMap(modelBuilder.Entity<User>());
        }
    }
}
