using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderService.Model;
using System.Text.Json;

namespace OrderService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly RabbitMQPublisher _publisher;

        public OrderController(RabbitMQPublisher publisher)
        {
            _publisher = publisher;
        }

        [HttpPost]
        public IActionResult PlaceOrder([FromBody] Order order)
        {
            _publisher.Publish($"{order.Email}");
            return Ok("Order placed successfully.");
        }
    }
}
