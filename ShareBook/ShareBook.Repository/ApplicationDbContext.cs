﻿using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShareBook.Domain;
using ShareBook.Repository.Mapping;

namespace ShareBook.Repository
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public ApplicationDbContext() { }

        public DbSet<Book> Books { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<LogEntry> LogEntries { get; set; }
        public DbSet<BookUser> BookUser { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<JobHistory> JobHistories { get; set; }
        public DbSet<AccessHistory> AccessHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            new BookMap(modelBuilder.Entity<Book>());
            new UserMap(modelBuilder.Entity<User>());
            new BookUserMap(modelBuilder.Entity<BookUser>());
            new CategoryMap(modelBuilder.Entity<Category>());
            new AddressMap(modelBuilder.Entity<Address>());
            new JobHistoryMap(modelBuilder.Entity<JobHistory>());

            //O Contexto procura pelas classes que implementam IEntityTypeConfiguration adicionando o mapeamento de forma automática.
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await this.LogChanges();
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
