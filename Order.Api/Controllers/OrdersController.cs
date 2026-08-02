using System.Security.Claims;
using Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Order.Api.Services;

namespace Order.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
  
    public class OrdersController : ControllerBase
    {
        private readonly IOrderServices _orderServices;
        private readonly ProductClientService _productClient;
        private readonly UserClientService _userClient;
        public OrdersController(IOrderServices orderServices, ProductClientService productClient, UserClientService userClient)
        {
            _orderServices = orderServices;
            _productClient = productClient;
            _userClient = userClient;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var orders = await _orderServices.GetOrders();
            var ordersWithUser = new List<object>();

            foreach (var o in orders)
            {
                var user = await _userClient.GetUserDetailsAsync(o.CreatedBy);
                var  product=await _productClient.GetProductDetailsAsync(o.ProductId);
                ordersWithUser.Add(new
                {
                    o.Id,
                    o.ProductId,
                    o.Quantity,
                    CustomerName = user.Exists ? user.FullName : "Unknown User",
                    productName= product?.Name?? "Unknown Product",
                });
            }

            return Ok(ordersWithUser);
        }

        [HttpGet("two")]
        public async Task<IActionResult> two()
        {
            var orders = await _orderServices.GetOrders();
            var ordersWithUser = new List<object>();

            foreach (var o in orders)
            {

               
                var user = await _userClient.GetUserDetailsAsync(o.CreatedBy);
                var product = await _productClient.GetProductDetailsAsync(o.ProductId);
                ordersWithUser.Add(new
                {
                    o.Id,
                    o.ProductId,
                    o.Quantity,
                    CustomerName = user.Exists ? user.FullName : "Unknown User",
                    productName = product?.Name ?? "Unknown Product",
                });
            }

            return Ok(ordersWithUser);
        }

        [HttpGet("Three")]
        public async Task<IActionResult> three()
        {
            var orders = await _orderServices.GetOrders();
            var ordersWithUser = new List<object>();

            foreach (var o in orders)
            {
                var user = await _userClient.GetUserDetailsAsync(o.CreatedBy);
                var product = await _productClient.GetProductDetailsAsync(o.ProductId);
                ordersWithUser.Add(new
                {
                    o.Id,
                    o.ProductId,
                    o.Quantity,
                    CustomerName = user.Exists ? user.FullName : "Unknown User",
                    productName = product?.Name ?? "Unknown Product",
                });
            }

            return Ok(ordersWithUser);
        }
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var order =await _orderServices.GetOrder(id);            
            return Ok(order);
        }
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderDto order)
        {
            // Check stock via gRPC
            var (isAvailable, message) = await _productClient.CheckProductStockAsync(order.ProductId, order.Quantity);
            
            if (!isAvailable)
            {
                return BadRequest(new { Message = message });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var orderCreated = await _orderServices.CreateOrder(order.ProductId, order.Quantity, 
                userId != null ? Guid.Parse(userId) : Guid.Empty);
            return Ok(orderCreated);
        }
        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> UpdateOrder(Guid id, [FromBody] OrderDto order)
        {
            await _orderServices.UpdateOrder(id,order.ProductId, order.Quantity);
            return Ok();
        }
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteOrder(Guid id)
        {
            var isDeleted = await _orderServices.DeleteOrder(id);
            if (!isDeleted)
            {
                return NotFound();
            }

            return Ok(new { Message = $"Order with ID: {id} deleted successfully." });

        }

    }
    public class OrderDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }

}

