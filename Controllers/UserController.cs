using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LMS_backend.Dtos;
using LMS_backend.Services.Interfaces;
using LMS_backend.Entities;

namespace LMS_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            try
            {
                var response = await _userService.RegisterAsync(registerDto);
                return CreatedAtAction(
                    nameof(GetUserById), 
                    new { id = response.User.Id }, 
                    new { message = "User registered successfully", data = response }
                );
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Registration failed", error = ex.Message });
            }
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
        {
            try
            {
                var response = await _userService.LoginAsync(loginDto);
                return Ok(new { message = "Login successful", data = response });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Login failed", error = ex.Message });
            }
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> SendPasswordResetCode([FromBody] SendPasswordResetCodeDto request)
        {
            var result = await _userService.SendPasswordResetCodeAsync(request.Email);
            if (!result)
                return NotFound(new { message = "User not found" });

            return Ok(new { 
                message = "If the email exists in our system, a password reset code has been sent. Please check your email." 
            });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordDto request)
        {
            try
            {
                var result = await _userService.ResetPasswordAsync(request);
                if (!result)
                    return NotFound(new { message = "Invalid email or reset code" });

                return Ok(new { message = "Password has been reset successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(new { message = "Users retrieved successfully", data = users });
        }

        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound(new { message = "Current user not found" });
            return Ok(new { message = "Current user retrieved successfully", data = user });
        }

        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserDto>> GetUserById(Guid id)
        {
            try
            {
                var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var isAdmin = User.IsInRole("Admin");

                if (!isAdmin && currentUserId != id)
                    return Forbid();

                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound(new { message = $"User with ID {id} not found" });

                return Ok(new { message = "User retrieved successfully", data = user });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to retrieve user", error = ex.Message });
            }
        }

        [HttpGet("email/{email}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserDto>> GetUserByEmail(string email)
        {
            var user = await _userService.GetUserByEmailAsync(email);            if (user == null)
                return NotFound(new { message = "User not found" });
            return Ok(new { message = "User retrieved successfully", data = user });
        }

        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserDto>> UpdateUser(Guid id, UpdateUserDto updateUserDto)
        {
            try
            {
                var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var isAdmin = User.IsInRole("Admin");

                if (!isAdmin && currentUserId != id)
                    return Forbid();

                var user = await _userService.UpdateUserAsync(id, updateUserDto);
                if (user == null)
                    return NotFound(new { message = $"User with ID {id} not found" });

                return Ok(new { message = "User updated successfully", data = user });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to update user", error = ex.Message });
            }
        }

        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                await _userService.ChangePasswordAsync(userId, changePasswordDto);
                return Ok(new { message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to change password", error = ex.Message });
            }
        }        [HttpPost("bookmarks/{bookId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> ToggleBookmark(Guid bookId)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await _userService.ToggleBookmarkAsync(userId, bookId);
                return Ok(new { message = result ? "Book bookmarked successfully" : "Book removed from bookmarks", data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to toggle bookmark", error = ex.Message });
            }
        }

        [HttpGet("bookmarks")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetBookmarks()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var bookmarks = await _userService.GetBookmarksAsync(userId);
                return Ok(new { message = "Bookmarks retrieved successfully", data = bookmarks });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to retrieve bookmarks", error = ex.Message });
            }
        }

        [HttpPost("{id}/activate")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> ActivateUser(Guid id)
        {
            try
            {
                var result = await _userService.ActivateUserAsync(id);
                if (!result)
                    return BadRequest(new { message = "User is already active" });
                return Ok(new { message = "User activated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to activate user", error = ex.Message });
            }
        }

        [HttpPost("{id}/deactivate")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> DeactivateUser(Guid id)
        {
            try
            {
                var result = await _userService.DeactivateUserAsync(id);
                if (!result)
                    return BadRequest(new { message = "User is already inactive" });
                return Ok(new { message = "User deactivated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to deactivate user", error = ex.Message });
            }
        }

        [HttpPatch("{id}/role")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<UserDto>> UpdateRole(Guid id, [FromBody] UserRole role)
        {
            try
            {
                var user = await _userService.UpdateRoleAsync(id, role);
                return Ok(new { message = "User role updated successfully", data = user });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to update user role", error = ex.Message });
            }
        }
        
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteUser(Guid id)
        {
            try
            {
                var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var isAdmin = User.IsInRole("Admin");

                if (!isAdmin && currentUserId != id)
                    return Forbid();

                var result = await _userService.DeleteUserAsync(id);
                if (!result)
                    return NotFound(new { message = $"User with ID {id} not found" });

                return Ok(new { message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to delete user", error = ex.Message });
            }
        }
    }
}