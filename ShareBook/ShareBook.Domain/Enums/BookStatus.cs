
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel;

namespace ShareBook.Domain.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BookStatus
    {
        [Description("Aguardando aprovação")]
        WaitingApproval,
        [Description("Disponível")]
        Available,
        [Description("Invisível")]
        Invisible,
        [Description("Doado")]
        Donated,
        [Description("Cancelado")]
        Canceled
    }
}
