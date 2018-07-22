using JsonDiffPatchDotNet;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ShareBook.Repository
{
    public static class LoggingContext
    {
        private static readonly List<EntityState> entityStates = new List<EntityState>() { EntityState.Added, EntityState.Modified, EntityState.Deleted };

        public static async Task LogChanges(this ApplicationDbContext context)
        {
            var logTime = DateTime.Now;
            const string emptyJson = "{}";
            const string idColumn = "Id";

            Guid? user = null;
            if (!string.IsNullOrEmpty(Thread.CurrentPrincipal?.Identity?.Name))
                user = new Guid(Thread.CurrentPrincipal?.Identity?.Name);

            var changes = context.ChangeTracker.Entries()
                .Where(x => entityStates.Contains(x.State) && x.Entity.GetType().IsSubclassOf(typeof(BaseEntity)))
                .ToList();

            var jdp = new JsonDiffPatch();

            foreach (var item in changes)
            {
                var original = emptyJson;
                var updated = JsonConvert.SerializeObject(item.CurrentValues.Properties.ToDictionary(pn => pn.Name, pn => item.CurrentValues[pn]));
                var creationDate = DateTime.Now;

                if (item.State == EntityState.Modified)
                {
                    var dbValues = await item.GetDatabaseValuesAsync();
                    original = JsonConvert.SerializeObject(dbValues.Properties.ToDictionary(pn => pn.Name, pn => dbValues[pn]));

                    creationDate = dbValues.GetValue<DateTime>("CreationDate");
                }

                item.Property("CreationDate").CurrentValue = creationDate;

                string jsonDiff = jdp.Diff(original, updated);

                if (string.IsNullOrWhiteSpace(jsonDiff) == false)
                {
                    var EntityDiff = JToken.Parse(jsonDiff).ToString(Formatting.None);

                    var logEntry = new LogEntry()
                    {
                        EntityName = item.Entity.GetType().Name,
                        EntityId = new Guid(item.CurrentValues[idColumn].ToString()),
                        LogDateTime = logTime,
                        Operation = item.State.ToString(),
                        UserId = user,
                        ValuesChanges = EntityDiff,
                    };

                    context.LogEntries.Add(logEntry);
                }
                
            }
        }
    }
}
