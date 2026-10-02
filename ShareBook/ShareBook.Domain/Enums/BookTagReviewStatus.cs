using System.ComponentModel;
using System.Text.Json.Serialization;

namespace ShareBook.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BookTagReviewStatus
{
    [Description("Aprovada")]
    Approved,

    [Description("Pendente")]
    Pending,

    [Description("Rejeitada")]
    Rejected
}
