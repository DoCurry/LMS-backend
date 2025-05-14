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
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<CartDto>> GetCart()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var cart = await _cartService.GetCartAsync(userId);
            return Ok(new { message = "Cart retrieved successfully", data = cart });
        }

        [HttpGet("summary")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<CartSummaryDto>> GetCartSummary()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var summary = await _cartService.GetCartSummaryAsync(userId);
            return Ok(new { message = "Cart summary retrieved successfully", data = summary });
        }

        [HttpPost("items")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CartDto>> AddToCart(AddToCartDto addToCartDto)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var cart = await _cartService.AddToCartAsync(userId, addToCartDto);
                if (cart == null)
                    return NotFound(new { message = "Book not found or not available" });
                return Ok(new { message = "Item added to cart successfully", data = cart });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to add item to cart", error = ex.Message });
            }
        }

        [HttpPut("items/{itemId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CartDto>> UpdateCartItem(Guid itemId, UpdateCartItemDto updateCartItemDto)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var cart = await _cartService.UpdateCartItemAsync(userId, itemId, updateCartItemDto);
                if (cart == null)
                    return NotFound(new { message = $"Cart item with ID {itemId} not found" });
                return Ok(new { message = "Cart item updated successfully", data = cart });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to update cart item", error = ex.Message });
            }
        }        [HttpDelete("items/{itemId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> RemoveFromCart(Guid itemId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _cartService.RemoveFromCartAsync(userId, itemId);
            if (!result)
                return NotFound(new { message = $"Cart item with ID {itemId} not found" });
            return Ok(new { message = "Item removed from cart successfully" });
        }        [HttpDelete("clear")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> ClearCart()
        {            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _cartService.ClearCartAsync(userId);
            if (!result)
                return BadRequest(new { message = "Failed to clear cart" });
            return Ok(new { message = "Cart cleared successfully" });
        }
    }
}