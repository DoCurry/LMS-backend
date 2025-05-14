using LMS_backend.Dtos;
using LMS_backend.Entities;

namespace LMS_backend.Services.Interfaces
{
    public interface IUserService
    {
        // Authentication
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto);
        Task<bool> SendPasswordResetCodeAsync(string email);
        Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
        
        // User management
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByIdAsync(Guid id);
        Task<UserDto?> GetUserByEmailAsync(string email);
        Task<UserDto?> UpdateUserAsync(Guid userId, UpdateUserDto updateUserDto);
        Task<bool> DeleteUserAsync(Guid id);
        Task<bool> ActivateUserAsync(Guid id);        Task<bool> DeactivateUserAsync(Guid id);
        Task<UserDto> UpdateRoleAsync(Guid id, UserRole role);
        
        // Bookmarks
        Task<bool> ToggleBookmarkAsync(Guid userId, Guid bookId);
        Task<IEnumerable<BookDto>> GetBookmarksAsync(Guid userId);
    }
}