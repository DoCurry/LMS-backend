using LMS_backend.Dtos;

namespace LMS_backend.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendOrderConfirmationAsync(OrderResponseDto order);
        Task SendOrderCancellationAsync(OrderDto order);
        Task SendOrderReadyForPickupAsync(OrderDto order);
        Task SendPasswordResetEmailAsync(string email, string resetCode);
    }
}