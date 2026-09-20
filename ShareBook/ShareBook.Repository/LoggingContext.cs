using JsonDiffPatchDotNet;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShareBook.Repository;

public static class LoggingContext
{
    private static readonly List<EntityState> entityStates = new List<EntityState>() { EntityState.Added, EntityState.Modified, EntityState.Deleted };

    /// <summary>
    /// Registra as mudanças pendentes no ChangeTracker como EFLog antes do SaveChanges de fato.
    /// </summary>
    /// <param name="currentUserId">
    /// Id do usuário autenticado que causou a mudança, vindo de <see cref="Domain.Common.ICurrentUserAccessor"/>
    /// (ver <see cref="ApplicationDbContext"/>). Null fora de um request HTTP (job em background,
    /// tooling de design-time) — o EFLog fica sem autor nesses casos, como sempre foi.
    /// </param>
    public static async Task LogChanges(this ApplicationDbContext context, Guid? currentUserId = null)
    {
        var logTime = DateTime.UtcNow;
        const string emptyJson = "{}";
        const string idColumn = "Id";

        var user = currentUserId;

        var changes = context.ChangeTracker.Entries()
            .Where(x => entityStates.Contains(x.State) && x.Entity.GetType().IsSubclassOf(typeof(BaseEntity)))
            .ToList();

        var jdp = new JsonDiffPatch();

        foreach (var item in changes)
        {
            var original = emptyJson;
            var updated = JsonConvert.SerializeObject(item.CurrentValues.Properties.ToDictionary(pn => pn.Name, pn => item.CurrentValues[pn]));
            var creationDate = DateTime.UtcNow;

            if (item.State == EntityState.Modified)
            {
                var dbValues = await item.GetDatabaseValuesAsync();

                if (dbValues != null)
                {
                    original = JsonConvert.SerializeObject(dbValues.Properties.ToDictionary(pn => pn.Name, pn => dbValues[pn]));
                    creationDate = dbValues.GetValue<DateTime>("CreationDate");
                }
            }

            item.Property("CreationDate").CurrentValue = creationDate;

            string jsonDiff = jdp.Diff(original, updated);

            if (string.IsNullOrWhiteSpace(jsonDiff) == false)
            {
                var EntityDiff = JToken.Parse(jsonDiff).ToString(Formatting.None);

                var efLog = new EFLog()
                {
                    EntityName = item.Entity.GetType().Name,
                    EntityId = new Guid(item.CurrentValues[idColumn]!.ToString()!),
                    LogDateTime = logTime,
                    Operation = item.State.ToString(),
                    UserId = user,
                    ValuesChanges = EntityDiff,
                };

                context.EFLogs.Add(efLog);
            }
            
        }
    }
}
