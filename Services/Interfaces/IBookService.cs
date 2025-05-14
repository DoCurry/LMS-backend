using LMS_backend.Dtos;
using LMS_backend.Entities;

namespace LMS_backend.Services.Interfaces
{
    public interface IBookService
    {
        Task<IEnumerable<BookDto>> GetAllBooksAsync();
        Task<BookDto?> GetBookByIdAsync(Guid id);
        Task<BookDto?> GetBookByISBNAsync(string isbn);
        Task<BookDto?> GetBookBySlugAsync(string slug);
        Task<IEnumerable<BookDto>> GetBooksByFilterAsync(BookFilterDto filter);
        Task<IEnumerable<BookDto>> GetBestSellersAsync(int limit = 10);
        Task<IEnumerable<BookDto>> GetNewReleasesAsync();
        Task<IEnumerable<BookDto>> GetNewArrivalsAsync();
        Task<IEnumerable<BookDto>> GetComingSoonAsync();
        Task<IEnumerable<BookDto>> GetDealsAsync();
        Task<BookDto> CreateBookAsync(CreateBookDto createBookDto);
        Task<BookDto?> UpdateBookAsync(Guid id, UpdateBookDto updateBookDto);
        Task<bool> DeleteBookAsync(Guid id);
        Task<bool> UpdateStockAsync(Guid id, int quantity);
        Task<bool> SetDiscountAsync(Guid id, decimal percentage, DateTime? startDate, DateTime? endDate);
        Task<bool> RemoveDiscountAsync(Guid id);
        Task<double> GetAverageRatingAsync(Guid id);
        Task<bool> IsAvailableAsync(Guid id);        Task<bool> UpdateCoverImageAsync(Guid id, IFormFile image);
        Task<bool> IsBookmarkedByUserAsync(Guid bookId, Guid userId);
    }
}