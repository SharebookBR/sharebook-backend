using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel;

namespace ShareBook.Domain.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
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
