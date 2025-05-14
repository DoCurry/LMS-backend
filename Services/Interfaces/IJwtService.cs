using LMS_backend.Entities;

namespace LMS_backend.Services.Interfaces
{
    public interface IJwtService
    {
        Task<string> GenerateTokenAsync(User user);
        Task<bool> ValidateTokenAsync(string token);
        Task<Guid?> GetUserIdFromTokenAsync(string token);
    }
}