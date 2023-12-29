using MassTransit;
using Messages.Order.Event;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging; // Add this using statement
using OrderService.Models;
using System;
using System.Threading.Tasks;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IBus _bus;
        private readonly ILogger<OrderController> _logger;

        public OrderController(ILogger<OrderController> logger, IBus bus)
        {
            _logger = logger;
            _bus = bus;
        }

        [HttpPost]
        [Route("CreateOrder")]
        public async Task<IActionResult> CreateOrder([FromBody] OrderPrice orderModel)
        {
            try
            {
                // Log the start of the action
                _logger.LogInformation("CreateOrder action started - OrderId: {OrderId}", orderModel.OrderId);

                await _bus.Publish<IOrderStartedEvent>(new
                {
                    OrderId = orderModel.OrderId,
                    PaymentCardNumber = orderModel.PaymentCardNumber,
                    ProductName = orderModel.ProductName,
                    IsCanceled = orderModel.IsCanceled
                });

                // Log the success
                _logger.LogInformation("CreateOrder action completed successfully - OrderId: {OrderId}", orderModel.OrderId);

                return Ok("Success");
            }
            catch (Exception ex)
            {
                // Log and handle the exception
                _logger.LogError(ex, "An exception occurred in CreateOrder action - OrderId: {OrderId}", orderModel.OrderId);
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}
