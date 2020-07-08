using System;

namespace ShareBook.Api.ViewModels
{
    public class MyBookRequestVM
    {
        public Guid RequestId { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }

        // status do pedido. (ex: aguardando decisão, negado, doado)
        // apenas pra saber se ganhou ou não.
        public string Status { get; set; }

        // status da doação. (ex: aguardando envio, enviado, recebido)
        // para saber sobre o envio.
        public string BookStatus { get; set; }

        public string TrackingNumber { get; set; }
    }
}
