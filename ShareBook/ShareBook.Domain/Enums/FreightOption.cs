using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel;

namespace ShareBook.Domain.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum FreightOption
    {
        [Description("Não desejo pagar frete")]
        WithoutFreight,

        [Description("Sim, para toda cidade")]
        City,

        [Description("Sim, para todo estado")]
        State,

        [Description("Sim, para todo o Brasil")]
        Country,

        [Description("Sim, para todo o Mundo")]
        World
    }
}
