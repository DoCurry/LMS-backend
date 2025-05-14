using LMS_backend.Dtos;

namespace LMS_backend.Services.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
        Task<OrderDto?> GetOrderByIdAsync(Guid id);
        Task<IEnumerable<OrderDto>> GetUserOrdersAsync(Guid userId);
        Task<OrderResponseDto> CreateOrderAsync(Guid userId, CreateOrderDto createOrderDto);
        Task<OrderResponseDto> CreateOrderFromCartAsync(Guid userId);
        Task<bool> CancelOrderAsync(Guid orderId);
        Task<bool> CompleteOrderAsync(Guid orderId, string membershipId);
        Task<decimal> CalculateDiscountAsync(Guid userId, decimal subTotal, int itemCount);
        Task<bool> ValidateClaimCodeAsync(Guid orderId, string claimCode);
        Task<OrderDto?> GetOrderByClaimCodeAsync(string claimCode);
        Task<bool> SetOrderReadyForPickupAsync(Guid orderId);
    }
}