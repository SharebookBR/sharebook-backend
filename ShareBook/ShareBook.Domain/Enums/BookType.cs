using System.ComponentModel;
using System.Text.Json.Serialization;

namespace ShareBook.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum BookType
    {
        [Description("Impresso")]
        Printed,
        [Description("Eletrônico")]
        Eletronic
    }
}
