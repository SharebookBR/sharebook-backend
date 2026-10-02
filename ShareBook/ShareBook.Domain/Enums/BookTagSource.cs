using System.ComponentModel;
using System.Text.Json.Serialization;

namespace ShareBook.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BookTagSource
{
    [Description("Manual")]
    Manual,

    [Description("Assistida")]
    Assisted,

    [Description("Backfill")]
    Backfill
}
