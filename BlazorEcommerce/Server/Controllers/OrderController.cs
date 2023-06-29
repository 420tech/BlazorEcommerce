using Microsoft.AspNetCore.Mvc;

namespace BlazorEcommerce.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }


        [HttpGet]
        public async Task<ActionResult<ServiceResponse<List<OrderOverviewResponse>>>> GetOrders()
        {
            return Ok(await _orderService.GetOrders());
        }

        [HttpGet("{orderId}")]
        public async Task<ActionResult<OrderDetailsProductResponse>> GetOrderDetails(int orderId)
        {
            var result = await _orderService.GetOrderDetails(orderId);
            return Ok(result);
        }
    }
}
