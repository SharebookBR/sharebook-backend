using System.ComponentModel;

namespace ShareBook.Domain.Enums {
    public enum VisitorProfile {
        [Description("Doador")] Donor,
        [Description("Ganhador")] Winner,
        [Description("Indefinido")] Undefined,
        [Description("Facilitador")] Facilitator
    }
}