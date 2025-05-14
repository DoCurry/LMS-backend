using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using LMS_backend.Data;
using LMS_backend.Dtos;
using LMS_backend.Entities;
using LMS_backend.Services.Interfaces;

#nullable enable

namespace LMS_backend.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IJwtService _jwtService;
        private readonly IEmailService _emailService;

        public UserService(
            ApplicationDbContext context, 
            IMapper mapper, 
            IJwtService jwtService,
            IEmailService emailService)
        {
            _context = context;
            _mapper = mapper;
            _jwtService = jwtService;
            _emailService = emailService;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _context.Users
                .Include(u => u.Orders)
                .Include(u => u.Reviews)
                .Include(u => u.Bookmarks)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == loginDto.Email.ToLower());

            if (user == null)
                throw new Exception("User not found");

            if (!VerifyPasswordHash(loginDto.Password, user.PasswordHash))
                throw new Exception("Invalid password");

            if (!user.IsActive)
                throw new Exception("Account is deactivated");

            var token = await _jwtService.GenerateTokenAsync(user);
            var userDto = _mapper.Map<UserDto>(user) ?? throw new Exception("Failed to map user data");
            
            return new AuthResponseDto
            {
                Token = token,
                RefreshToken = string.Empty,
                User = userDto,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == registerDto.Email.ToLower()))
                throw new Exception("Email already registered");

            if (await _context.Users.AnyAsync(u => u.Username.ToLower() == registerDto.Username.ToLower()))
                throw new Exception("Username already taken");

            var user = _mapper.Map<User>(registerDto) ?? throw new Exception("Failed to map user data");
            user.PasswordHash = HashPassword(registerDto.Password);
            user.MembershipId = await GenerateUniqueMembershipIdAsync();
            user.Role = UserRole.Member;
            user.IsActive = true;
            user.CreatedAt = DateTime.UtcNow;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = await _jwtService.GenerateTokenAsync(user);
            var userDto = _mapper.Map<UserDto>(user) ?? throw new Exception("Failed to map user data");
            
            return new AuthResponseDto
            {
                Token = token,
                RefreshToken = string.Empty,
                User = userDto,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new Exception("User not found");

            if (!VerifyPasswordHash(changePasswordDto.CurrentPassword, user.PasswordHash))
                throw new Exception("Current password is incorrect");

            user.PasswordHash = HashPassword(changePasswordDto.NewPassword);
            user.LastUpdated = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _context.Users
                .Include(u => u.Orders)
                .Include(u => u.Reviews)
                .Include(u => u.Bookmarks)
                .ToListAsync();
            
            return users.Select(u => _mapper.Map<UserDto>(u) ?? throw new Exception("Failed to map user data")).ToList();
        }

        public async Task<UserDto?> GetUserByIdAsync(Guid id)
        {
            var user = await _context.Users
                .Include(u => u.Orders)
                .Include(u => u.Reviews)
                .Include(u => u.Bookmarks)
                .FirstOrDefaultAsync(u => u.Id == id);
            
            return user == null ? null : _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            var user = await _context.Users
                .Include(u => u.Orders)
                .Include(u => u.Reviews)
                .Include(u => u.Bookmarks)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
            
            return user == null ? null : _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto?> UpdateUserAsync(Guid userId, UpdateUserDto updateUserDto)
        {
            var user = await _context.Users
                .Include(u => u.Orders)
                .Include(u => u.Reviews)
                .Include(u => u.Bookmarks)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new Exception("User not found");

            if (updateUserDto.Email != null && 
                await _context.Users.AnyAsync(u => u.Email.ToLower() == updateUserDto.Email.ToLower() && u.Id != userId))
                throw new Exception("Email already registered");

            if (updateUserDto.Username != null && 
                await _context.Users.AnyAsync(u => u.Username.ToLower() == updateUserDto.Username.ToLower() && u.Id != userId))
                throw new Exception("Username already taken");

            _mapper.Map(updateUserDto, user);
            user.LastUpdated = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return _mapper.Map<UserDto>(user);
        }

        public async Task<bool> DeleteUserAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleBookmarkAsync(Guid userId, Guid bookId)
        {
            var user = await _context.Users
                .Include(u => u.Bookmarks)
                .FirstOrDefaultAsync(u => u.Id == userId);
                
            if (user == null)
                throw new Exception("User not found");

            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
                throw new Exception("Book not found");

            var bookmark = user.Bookmarks.FirstOrDefault(b => b.Id == bookId);
            if (bookmark == null)
            {
                user.Bookmarks.Add(book);
                await _context.SaveChangesAsync();
                return true;
            }
            else
            {
                user.Bookmarks.Remove(bookmark);
                await _context.SaveChangesAsync();
                return false;
            }
            
        }

        public async Task<IEnumerable<BookDto>> GetBookmarksAsync(Guid userId)
        {
            var user = await _context.Users
                .Include(u => u.Bookmarks)
                .ThenInclude(b => b.Reviews)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new Exception("User not found");

            return user.Bookmarks.Select(b => _mapper.Map<BookDto>(b) ?? throw new Exception("Failed to map book data")).ToList();
        }

        public async Task<bool> ActivateUserAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                throw new Exception("User not found");

            if (user.IsActive)
                return false; // Already active

            user.IsActive = true;
            user.LastUpdated = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }        public async Task<bool> DeactivateUserAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                throw new Exception("User not found");

            if (!user.IsActive)
                return false; // Already inactive

            user.IsActive = false;
            user.LastUpdated = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<UserDto> UpdateRoleAsync(Guid id, UserRole role)
        {
            var user = await _context.Users
                .Include(u => u.Orders)
                .Include(u => u.Reviews)
                .Include(u => u.Bookmarks)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                throw new Exception("User not found");

            user.Role = role;
            user.LastUpdated = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return _mapper.Map<UserDto>(user);
        }

        public async Task<bool> SendPasswordResetCodeAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return false;

            // Generate a 6-digit reset code
            var resetCode = new Random().Next(100000, 999999).ToString();
            user.PasswordResetCode = resetCode;
            user.PasswordResetCodeExpiry = DateTime.UtcNow.AddMinutes(15);
            user.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _emailService.SendPasswordResetEmailAsync(email, resetCode);

            return true;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => 
                u.Email == resetPasswordDto.Email && 
                u.PasswordResetCode == resetPasswordDto.ResetCode);

            if (user == null)
                return false;

            if (user.PasswordResetCodeExpiry == null || user.PasswordResetCodeExpiry < DateTime.UtcNow)
                throw new Exception("Reset code has expired");

            var passwordHash = HashPassword(resetPasswordDto.NewPassword);
            user.PasswordHash = passwordHash;
            user.PasswordResetCode = null;
            user.PasswordResetCodeExpiry = null;
            user.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<string> GenerateUniqueMembershipIdAsync()
        {
            Random random = new();
            string membershipId;
            bool isUnique = false;

            do
            {
                // Generate a 5-digit number
                membershipId = random.Next(10000, 100000).ToString();
                isUnique = !await _context.Users.AnyAsync(u => u.MembershipId == membershipId);
            } while (!isUnique);

            return membershipId;
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private static bool VerifyPasswordHash(string password, string hash)
        {
            return HashPassword(password) == hash;
        }
    }
}