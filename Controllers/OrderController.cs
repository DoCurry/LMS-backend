using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LMS_backend.Dtos;
using LMS_backend.Services.Interfaces;

namespace LMS_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // Add this private helper method
        private Guid GetUserIdFromClaims()
        {
            return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(new { message = "All orders retrieved successfully", data = orders });
        }        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderDto>> GetOrderById(Guid id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var isAdmin = User.IsInRole("Admin");
            var isStaff = User.IsInRole("Staff");

            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound(new { message = $"Order with ID {id} not found" });

            if (!isAdmin && !isStaff && order.User.Id != userId)
                return Forbid();

            return Ok(new { message = "Order retrieved successfully", data = order });
        }

        [HttpGet("user")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetUserOrders()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var orders = await _orderService.GetUserOrdersAsync(userId);
            return Ok(new { message = "User orders retrieved successfully", data = orders });
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<OrderResponseDto>> CreateOrder(CreateOrderDto createOrderDto)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var order = await _orderService.CreateOrderAsync(userId, createOrderDto);
                return CreatedAtAction(
                    nameof(GetOrderById),
                    new { id = order.Id },
                    new { message = "Order created successfully", data = order }
                );
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to create order", error = ex.Message });
            }
        }

        [HttpPost("from-cart")]
        [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<OrderResponseDto>> CreateOrderFromCart()
        {
            try
            {
                var userId = GetUserIdFromClaims();
                var order = await _orderService.CreateOrderFromCartAsync(userId);
                return CreatedAtAction(
                    nameof(GetOrderById), 
                    new { id = order.Id }, 
                    new { message = "Order created successfully", data = order }
                );
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to create order from cart", error = ex.Message });
            }
        }        [HttpPost("{orderId}/cancel")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> CancelOrder(Guid orderId)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var isAdmin = User.IsInRole("Admin");
                var isStaff = User.IsInRole("Staff");

                var order = await _orderService.GetOrderByIdAsync(orderId);
                if (order == null)
                    return NotFound(new { message = $"Order with ID {orderId} not found" });

                if (!isAdmin && !isStaff && order.User.Id != userId)
                    return Forbid();

                var result = await _orderService.CancelOrderAsync(orderId);
                if (!result)
                    return BadRequest(new { message = "Order could not be cancelled" });

                return Ok(new { message = "Order cancelled successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to cancel order", error = ex.Message });
            }
        }        [HttpPost("{orderId}/complete")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> CompleteOrder(Guid orderId, [FromQuery] string membershipId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(membershipId))
                    return BadRequest(new { message = "Membership ID is required" });

                var order = await _orderService.GetOrderByIdAsync(orderId);
                if (order == null)
                    return NotFound(new { message = $"Order with ID {orderId} not found" });

                var result = await _orderService.CompleteOrderAsync(orderId, membershipId);
                if (!result)
                    return BadRequest(new { message = "Order could not be completed" });

                // Get updated order details
                var updatedOrder = await _orderService.GetOrderByIdAsync(orderId);
                if (updatedOrder == null)
                    return NotFound(new { message = "Order not found" });

                return Ok(new { message = "Order completed successfully", data = updatedOrder });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to complete order", error = ex.Message });
            }
        }        [HttpPost("{orderId}/ready-for-pickup")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> SetOrderReadyForPickup(Guid orderId)
        {
            try
            {
                var result = await _orderService.SetOrderReadyForPickupAsync(orderId);
                if (!result)
                    return NotFound(new { message = $"Order with ID {orderId} not found" });

                // Get updated order details
                var order = await _orderService.GetOrderByIdAsync(orderId);
                return Ok(new { message = "Order marked as ready for pickup", data = order });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to update order status", error = ex.Message });
            }
        }

        [HttpGet("claim/{claimCode}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderDto>> GetOrderByClaimCode(string claimCode)
        {
            var order = await _orderService.GetOrderByClaimCodeAsync(claimCode);
            if (order == null)
                return NotFound(new { message = $"Order with claim code {claimCode} not found" });

            return Ok(new { message = "Order retrieved successfully", data = order });
        }
    }
}