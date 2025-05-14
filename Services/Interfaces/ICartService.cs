using LMS_backend.Dtos;

namespace LMS_backend.Services.Interfaces
{
    public interface ICartService
    {
        Task<CartDto?> GetCartAsync(Guid userId);
        Task<CartDto> AddToCartAsync(Guid userId, AddToCartDto addToCartDto);
        Task<CartDto?> UpdateCartItemAsync(Guid userId, Guid cartItemId, UpdateCartItemDto updateCartItemDto);
        Task<bool> RemoveFromCartAsync(Guid userId, Guid cartItemId);
        Task<bool> ClearCartAsync(Guid userId);
        Task<CartSummaryDto> GetCartSummaryAsync(Guid userId);
    }
}