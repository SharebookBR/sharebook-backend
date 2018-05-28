using Microsoft.EntityFrameworkCore;
using ShareBook.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ShareBook.Repository
{
    public static class LoggingContext
    {
        private static readonly List<EntityState> entityStates = new List<EntityState>() { EntityState.Added, EntityState.Modified, EntityState.Deleted };

        public static void LogChanges(this ApplicationDbContext context)
        {
            var logTime = DateTime.Now;

            Guid? user = null;
            if (!string.IsNullOrEmpty(Thread.CurrentPrincipal?.Identity?.Name))
                user = new Guid(Thread.CurrentPrincipal?.Identity?.Name);

            var changes = context.ChangeTracker.Entries()
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

            context.LogEntries.AddRange(changes);
        }
    }
}
