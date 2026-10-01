using System.ComponentModel;

namespace ShareBook.Domain.Enums; 
public enum VisitorProfile {
    [Description("Pessoa doadora")] Donor,
    [Description("Pessoa ganhadora")] Winner,
    [Description("Indefinido")] Undefined,
    [Description("Facilitador")] Facilitator
}
