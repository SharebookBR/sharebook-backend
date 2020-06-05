
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel;

namespace ShareBook.Domain.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BookStatus
    {
        [Description("Aguardando aprovação")]
        WaitingApproval,// Status inicial

        [Description("Disponível")]
        Available,// Status é usado quando o admin aprova o livro

        [Description("Aguardando decisão do doador")]
        AwaitingDonorDecision,// Status para quando já expirou a qt de dias que o livro pode ficar na vitrine tendo pessoas ja interessadas em ganha-lo e o doador ainda nao escolheu o ganhador

        [Description("Aguardando envio")]
        WaitingSend,// Status é usado a partir do momento que o doador escolhe um ganhador

        [Description("Enviado")]
        Sent,// Status para quando o doador envia o livro

        [Description("Recebido")]
        Received,// Status para quando o ganhador recebe o livro

        [Description("Cancelado")]
        Canceled
    }
}
