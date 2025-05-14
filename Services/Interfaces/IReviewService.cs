using LMS_backend.Dtos;

namespace LMS_backend.Services.Interfaces
{
    public interface IReviewService
    {
        Task<IEnumerable<ReviewDto>> GetAllReviewsAsync();
        Task<ReviewDto?> GetReviewByIdAsync(Guid id);
        Task<IEnumerable<ReviewDto>> GetBookReviewsAsync(Guid bookId);
        Task<IEnumerable<ReviewDto>> GetUserReviewsAsync(Guid userId);
        Task<ReviewDto> CreateReviewAsync(Guid userId, CreateReviewDto createReviewDto);
        Task<ReviewDto?> UpdateReviewAsync(Guid userId, Guid reviewId, UpdateReviewDto updateReviewDto);
        Task<bool> DeleteReviewAsync(Guid userId, Guid reviewId);
        Task<bool> HasUserReviewedBookAsync(Guid userId, Guid bookId);
    }
}