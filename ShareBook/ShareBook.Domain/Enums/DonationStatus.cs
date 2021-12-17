using System.ComponentModel;
using System.Text.Json.Serialization;

namespace ShareBook.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DonationStatus
    {
        [Description("Aguardando Ação")]
        WaitingAction,

        [Description("Doado")]
        Donated,

        [Description("Não foi dessa vez")]
        Denied
    }
}
