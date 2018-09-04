
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel;

namespace ShareBook.Domain.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DonationStatus
    {
        [Description("Aguardando Ação")]
        WaitingAction,

        [Description("Doado")]
        Donated,

        [Description("Recusado")]
        Denied
    }
}
