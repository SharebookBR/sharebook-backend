using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareBook.Domain;

namespace ShareBook.Repository.Mapping
{
    public class LogEntryMap
    {
        public LogEntryMap(EntityTypeBuilder<LogEntry> entityBuilder)
        {
            entityBuilder.HasKey(t => t.Id);
            entityBuilder.Property(t => t.UserId);
            entityBuilder.Property(t => t.EntityName);
            entityBuilder.Property(t => t.EntityId);
            entityBuilder.Property(t => t.Operation);
            entityBuilder.Property(t => t.LogDateTime);
            entityBuilder.Property(t => t.OriginalValues);
            entityBuilder.Property(t => t.UpdatedValues);
        }
    }
}