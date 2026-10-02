using System.ComponentModel;
using System.Text.Json.Serialization;

namespace ShareBook.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TagStatus
{
    [Description("Ativa")]
    Active,

    [Description("Inativa")]
    Inactive,

    [Description("Depreciada")]
    Deprecated
}
