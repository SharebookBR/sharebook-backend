using System.ComponentModel;
using System.Text.Json.Serialization;

namespace ShareBook.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum FreightOption
    {
        [Description("Cidade")]
        City,

        [Description("Estado")]
        State,

        [Description("País")]
        Country,

        [Description("Mundo")]
        World,

        [Description("Não")]
        WithoutFreight,
    }
}
