using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Repository.Mapping;

namespace ShareBook.Repository
{
    public class ApplicationDbContext : DbContext
    {
        private readonly List<EntityState> entityStates = new List<EntityState>() { EntityState.Added, EntityState.Modified, EntityState.Deleted };

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public ApplicationDbContext() { }

        public DbSet<Book> Books { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<LogEntry> LogEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            new BookMap(modelBuilder.Entity<Book>());
            new UserMap(modelBuilder.Entity<User>());
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var logTime = DateTime.Now;

            Guid? user = null;
            if (!string.IsNullOrEmpty(Thread.CurrentPrincipal?.Identity?.Name))
                user = new Guid(Thread.CurrentPrincipal?.Identity?.Name);

            var changes = ChangeTracker.Entries()
                .Where(x => entityStates.Contains(x.State) && x.Entity.GetType().IsSubclassOf(typeof(BaseEntity)))
                .Select(t => new LogEntry()
                {
                    EntityName = t.Entity.GetType().Name,
                    EntityId = new Guid(t.CurrentValues["Id"].ToString()),
                    LogDateTime = logTime,
                    Operation = t.State.ToString(),
                    UserId = user,
                    OriginalValues = string.Join("\n", t.OriginalValues.Properties.ToDictionary(pn => pn.Name, pn => t.OriginalValues[pn])),
                    UpdatedValues = string.Join("\n", t.CurrentValues.Properties.ToDictionary(pn => pn.Name, pn => t.CurrentValues[pn])),
                })
                .ToList();

            LogEntries.AddRange(changes);

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
