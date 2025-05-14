using AutoMapper;
using Microsoft.EntityFrameworkCore;
using LMS_backend.Data;
using LMS_backend.Dtos;
using LMS_backend.Entities;
using LMS_backend.Services.Interfaces;

namespace LMS_backend.Services
{
    public class ReviewService : IReviewService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IOrderService _orderService;

        public ReviewService(ApplicationDbContext context, IMapper mapper, IOrderService orderService)
        {
            _context = context;
            _mapper = mapper;
            _orderService = orderService;
        }

        public async Task<IEnumerable<ReviewDto>> GetAllReviewsAsync()
        {
            var reviews = await _context.Reviews
                .Include(r => r.Book!)
                .ThenInclude(b => b!.Authors)
                .Include(r => r.Book!)
                .ThenInclude(b => b!.Publishers)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return reviews.Select(r => _mapper.Map<ReviewDto>(r)!)
                .Where(r => r != null)
                .ToList();
        }

        public async Task<ReviewDto?> GetReviewByIdAsync(Guid id)
        {
            var review = await _context.Reviews
                .Include(r => r.Book!)
                .ThenInclude(b => b!.Authors)
                .Include(r => r.Book!)
                .ThenInclude(b => b!.Publishers)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            return review == null ? null : _mapper.Map<ReviewDto>(review);
        }

        public async Task<IEnumerable<ReviewDto>> GetBookReviewsAsync(Guid bookId)
        {
            var reviews = await _context.Reviews
                .Include(r => r.Book!)
                .ThenInclude(b => b!.Authors)
                .Include(r => r.Book!)
                .ThenInclude(b => b!.Publishers)
                .Include(r => r.User)
                .Where(r => r.BookId == bookId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return reviews.Select(r => _mapper.Map<ReviewDto>(r)!)
                .Where(r => r != null)
                .ToList();
        }

        public async Task<IEnumerable<ReviewDto>> GetUserReviewsAsync(Guid userId)
        {
            var reviews = await _context.Reviews
                .Include(r => r.Book!)
                .ThenInclude(b => b!.Authors)
                .Include(r => r.Book!)
                .ThenInclude(b => b!.Publishers)
                .Include(r => r.User)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return reviews.Select(r => _mapper.Map<ReviewDto>(r)!)
                .Where(r => r != null)
                .ToList();
        }

        public async Task<ReviewDto> CreateReviewAsync(Guid userId, CreateReviewDto createReviewDto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new Exception("User not found");

            var book = await _context.Books.FindAsync(createReviewDto.BookId);
            if (book == null)
                throw new Exception("Book not found");

            // Verify if user has purchased and received the book
            if (!await HasUserPurchasedBookAsync(userId, createReviewDto.BookId))
                throw new Exception("You can only review books you have purchased and received");

            // Check if user has already reviewed this book
            if (await HasUserReviewedBookAsync(userId, createReviewDto.BookId))
                throw new Exception("User has already reviewed this book");

            var review = _mapper.Map<Review>(createReviewDto);
            if (review == null)
                throw new Exception("Failed to map review data");

            review.UserId = userId;
            review.CreatedAt = DateTime.UtcNow;

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            await _context.Entry(review)
                .Reference(r => r.Book)
                .LoadAsync();

            await _context.Entry(review)
                .Reference(r => r.User)
                .LoadAsync();

            var reviewDto = _mapper.Map<ReviewDto>(review);
            if (reviewDto == null)
                throw new Exception("Failed to map created review data");

            return reviewDto;
        }

        public async Task<ReviewDto?> UpdateReviewAsync(Guid userId, Guid reviewId, UpdateReviewDto updateReviewDto)
        {
            var review = await _context.Reviews
                .Include(r => r.Book)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId);

            if (review == null)
                return null;

            _mapper.Map(updateReviewDto, review);
            await _context.SaveChangesAsync();

            return _mapper.Map<ReviewDto>(review);
        }

        public async Task<bool> DeleteReviewAsync(Guid userId, Guid reviewId)
        {
            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId);

            if (review == null)
                return false;

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HasUserReviewedBookAsync(Guid userId, Guid bookId)
        {
            return await _context.Reviews
                .AnyAsync(r => r.UserId == userId && r.BookId == bookId);
        }

        private async Task<bool> HasUserPurchasedBookAsync(Guid userId, Guid bookId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == userId && !o.IsCancelled && o.Status == OrderStatus.Completed)
                .AnyAsync(o => o.OrderItems.Any(oi => oi.BookId == bookId));
        }
    }
}