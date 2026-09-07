using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace ShareBook.Api.Controllers
{
    /// <summary>
    /// Webhook de eventos de entrega/bounce do Stalwart.
    /// Por enquanto apenas reconhece o POST com 200 OK; a implementação real
    /// (parse do evento, alimentação de MailBounce e da lista de supressão) vem depois.
    /// </summary>
    [Route("api/[controller]")]
    [EnableCors("AllowAllHeaders")]
    public class BounceController : ControllerBase
    {
        [HttpPost]
        public IActionResult Receive()
        {
            // Placeholder: sem processamento por enquanto.
            return Ok();
        }
    }
}
