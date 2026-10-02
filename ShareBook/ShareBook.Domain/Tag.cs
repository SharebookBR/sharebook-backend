using ShareBook.Domain.Enums;
using System;

namespace ShareBook.Domain;

public class Tag
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string[] Aliases { get; set; } = [];

    public string Family { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? UsageNotes { get; set; }

    public TagStatus Status { get; set; } = TagStatus.Active;

    public bool IsPublic { get; set; } = true;

    public DateTime CreationDate { get; set; } = DateTime.UtcNow;

    public DateTime? UpdateDate { get; set; }

    public bool MatchesIdOrAlias(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalizedValue = value.Trim();
        if (string.Equals(Id, normalizedValue, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return Aliases.Any(alias => string.Equals(alias, normalizedValue, StringComparison.OrdinalIgnoreCase));
    }
}
