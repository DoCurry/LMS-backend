using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using LMS_backend.Data;
using LMS_backend.Dtos;
using LMS_backend.Entities;
using LMS_backend.Services.Interfaces;
using System.Text.RegularExpressions;

namespace LMS_backend.Services
{
    public class BookService : IBookService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICloudinaryService _cloudinaryService;

        public BookService(ApplicationDbContext context, IMapper mapper, ICloudinaryService cloudinaryService)
        {
            _context = context;
            _mapper = mapper;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<IEnumerable<BookDto>> GetAllBooksAsync()
        {
            var books = await _context.Books
                .Include(b => b.Authors)
                .Include(b => b.Publishers)
                .Include(b => b.Reviews)
                .Include(b => b.BookmarkedBy)
                .ToListAsync();

            return books.Select(b => _mapper.Map<BookDto>(b)!)
                .Where(b => b != null)
                .ToList();
        }

        public async Task<BookDto?> GetBookByIdAsync(Guid id)
        {
            var book = await _context.Books
                .Include(b => b.Authors)
                .Include(b => b.Publishers)
                .Include(b => b.Reviews)
                .Include(b => b.BookmarkedBy)
                .FirstOrDefaultAsync(b => b.Id == id);

            return book == null ? null : _mapper.Map<BookDto>(book);
        }

        public async Task<BookDto?> GetBookByISBNAsync(string isbn)
        {
            var book = await _context.Books
                .Include(b => b.Authors)
                .Include(b => b.Publishers)
                .Include(b => b.Reviews)
                .Include(b => b.BookmarkedBy)
                .FirstOrDefaultAsync(b => b.ISBN == isbn);

            return book == null ? null : _mapper.Map<BookDto>(book);
        }

        public async Task<BookDto?> GetBookBySlugAsync(string slug)
        {
            var book = await _context.Books
                .Include(b => b.Authors)
                .Include(b => b.Publishers)
                .Include(b => b.Reviews)
                .Include(b => b.BookmarkedBy)
                .FirstOrDefaultAsync(b => b.slug == slug);

            return book == null ? null : _mapper.Map<BookDto>(book);
        }

        public async Task<IEnumerable<BookDto>> GetBooksByFilterAsync(BookFilterDto filter)
        {
            var query = _context.Books
                .Include(b => b.Authors)
                .Include(b => b.Publishers)
                .Include(b => b.Reviews)
                .Include(b => b.BookmarkedBy)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.ToLower();
                query = query.Where(b =>
                    b.Title.ToLower().Contains(searchTerm) ||
                    b.ISBN.ToLower().Contains(searchTerm) ||
                    b.Description.ToLower().Contains(searchTerm) ||
                    b.Authors.Any(a => a.Name.ToLower().Contains(searchTerm)) ||
                    b.Publishers.Any(p => p.Name.ToLower().Contains(searchTerm))
                );
            }

            if (filter.AuthorIds?.Any() == true)
            {
                query = query.Where(b => b.Authors.Any(a => filter.AuthorIds.Contains(a.Id)));
            }

            if (filter.PublisherIds?.Any() == true)
            {
                query = query.Where(b => b.Publishers.Any(p => filter.PublisherIds.Contains(p.Id)));
            }

            if (filter.MinPrice.HasValue)
            {
                query = query.Where(b => b.Price >= filter.MinPrice.Value);
            }

            if (filter.MaxPrice.HasValue)
            {
                query = query.Where(b => b.Price <= filter.MaxPrice.Value);
            }

            if (filter.Genres?.Any() == true)
            {
                query = query.Where(b => filter.Genres.Contains(b.Genre));
            }

            if (filter.Languages?.Any() == true)
            {
                query = query.Where(b => filter.Languages.Contains(b.Language));
            }

            if (filter.Formats?.Any() == true)
            {
                query = query.Where(b => filter.Formats.Contains(b.Format));
            }

            if (filter.IsAvailableInLibrary.HasValue)
            {
                query = query.Where(b => b.IsAvailableInLibrary == filter.IsAvailableInLibrary.Value);
            }

            if (filter.HasDiscount.HasValue)
            {
                if (filter.HasDiscount.Value)
                {
                    query = query.Where(b => b.DiscountPercentage > 0 && 
                        (!b.DiscountStartDate.HasValue || b.DiscountStartDate.Value <= DateTime.UtcNow) &&
                        (!b.DiscountEndDate.HasValue || b.DiscountEndDate.Value >= DateTime.UtcNow));
                }
                else
                {
                    query = query.Where(b => !b.DiscountPercentage.HasValue ||
                        (b.DiscountStartDate.HasValue && b.DiscountStartDate.Value > DateTime.UtcNow) ||
                        (b.DiscountEndDate.HasValue && b.DiscountEndDate.Value < DateTime.UtcNow));
                }
            }

            if (filter.MinRating.HasValue)
            {
                query = query.Where(b => b.Reviews.Average(r => r.Rating) >= filter.MinRating.Value);
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(filter.SortBy))
            {
                query = filter.SortBy.ToLower() switch
                {
                    "title" => filter.SortDescending == true ? 
                        query.OrderByDescending(b => b.Title) : 
                        query.OrderBy(b => b.Title),
                    "price" => filter.SortDescending == true ? 
                        query.OrderByDescending(b => b.Price) : 
                        query.OrderBy(b => b.Price),
                    "publicationdate" => filter.SortDescending == true ? 
                        query.OrderByDescending(b => b.PublicationDate) : 
                        query.OrderBy(b => b.PublicationDate),
                    "popularity" => filter.SortDescending == true ? 
                        query.OrderByDescending(b => b.SoldCount) : 
                        query.OrderBy(b => b.SoldCount),
                    _ => query.OrderByDescending(b => b.CreatedAt)
                };
            }
            else
            {
                query = query.OrderByDescending(b => b.CreatedAt);
            }

            // Apply pagination
            query = query.Skip((filter.PageNumber - 1) * filter.PageSize)
                        .Take(filter.PageSize);

            var books = await query.ToListAsync();
            return books.Select(b => _mapper.Map<BookDto>(b)!)
                .Where(b => b != null)
                .ToList();
        }

        public async Task<IEnumerable<BookDto>> GetBestSellersAsync(int limit = 10)
        {
            var books = await _context.Books
                .Include(b => b.Authors)
                .Include(b => b.Publishers)
                .Include(b => b.Reviews)
                .OrderByDescending(b => b.SoldCount)
                .Take(limit)
                .ToListAsync();

            return books.Select(b => _mapper.Map<BookDto>(b)!)
                .Where(b => b != null)
                .ToList();
        }

        public async Task<IEnumerable<BookDto>> GetNewReleasesAsync()
        {
            var threeMonthsAgo = DateTime.UtcNow.AddMonths(-3);
            var books = await _context.Books
                .Include(b => b.Authors)
                .Include(b => b.Publishers)
                .Include(b => b.Reviews)
                .Where(b => b.PublicationDate >= threeMonthsAgo)
                .OrderByDescending(b => b.PublicationDate)
                .ToListAsync();

            return books.Select(b => _mapper.Map<BookDto>(b)!)
                .Where(b => b != null)
                .ToList();
        }

        public async Task<IEnumerable<BookDto>> GetNewArrivalsAsync()
        {
            var oneMonthAgo = DateTime.UtcNow.AddMonths(-1);
            var books = await _context.Books
                .Include(b => b.Authors)
                .Include(b => b.Publishers)
                .Include(b => b.Reviews)
                .Where(b => b.CreatedAt >= oneMonthAgo)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return books.Select(b => _mapper.Map<BookDto>(b)!)
                .Where(b => b != null)
                .ToList();
        }

        public async Task<IEnumerable<BookDto>> GetComingSoonAsync()
        {
            var books = await _context.Books
                .Include(b => b.Authors)
                .Include(b => b.Publishers)
                .Include(b => b.Reviews)
                .Where(b => b.PublicationDate > DateTime.UtcNow)
                .OrderBy(b => b.PublicationDate)
                .ToListAsync();

            return books.Select(b => _mapper.Map<BookDto>(b)!)
                .Where(b => b != null)
                .ToList();
        }

        public async Task<IEnumerable<BookDto>> GetDealsAsync()
        {
            var now = DateTime.UtcNow;
            var books = await _context.Books
                .Include(b => b.Authors)
                .Include(b => b.Publishers)
                .Include(b => b.Reviews)
                .Where(b => b.DiscountPercentage > 0 &&
                    (!b.DiscountStartDate.HasValue || b.DiscountStartDate.Value <= now) &&
                    (!b.DiscountEndDate.HasValue || b.DiscountEndDate.Value >= now))
                .OrderByDescending(b => b.DiscountPercentage)
                .ToListAsync();

            return books.Select(b => _mapper.Map<BookDto>(b)!)
                .Where(b => b != null)
                .ToList();
        }

        public async Task<BookDto> CreateBookAsync(CreateBookDto createBookDto)
        {
            if (await _context.Books.AnyAsync(b => b.ISBN == createBookDto.ISBN))
                throw new Exception("A book with this ISBN already exists");

            if (await _context.Books.AnyAsync(b => b.Title.ToLower() == createBookDto.Title.ToLower()))
                throw new Exception("A book with this title already exists");

            var book = _mapper.Map<Book>(createBookDto);
            if (book == null)
                throw new Exception("Failed to map book data");

            // Handle cover image upload
            if (createBookDto.CoverImage != null)
            {
                try
                {
                    var imageUrl = await _cloudinaryService.UploadImageAsync(createBookDto.CoverImage);
                    book.ImageUrl = imageUrl;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to upload cover image: {ex.Message}");
                }
            }

            // Fetch and assign authors
            var authors = await _context.Authors
                .Where(a => createBookDto.AuthorIds.Contains(a.Id))
                .ToListAsync();
            if (authors.Count != createBookDto.AuthorIds.Count)
                throw new Exception("One or more authors not found");
            book.Authors = authors;

            // Fetch and assign publishers
            var publishers = await _context.Publishers
                .Where(p => createBookDto.PublisherIds.Contains(p.Id))
                .ToListAsync();
            if (publishers.Count != createBookDto.PublisherIds.Count)
                throw new Exception("One or more publishers not found");
            book.Publishers = publishers;

            // Generate slug
            book.slug = GenerateSlug(createBookDto.Title);

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            var bookDto = _mapper.Map<BookDto>(book);
            if (bookDto == null)
                throw new Exception("Failed to map created book data");

            return bookDto;
        }

        public async Task<BookDto?> UpdateBookAsync(Guid id, UpdateBookDto updateBookDto)
        {
            var book = await _context.Books
                .Include(b => b.Authors)
                .Include(b => b.Publishers)
                .Include(b => b.Reviews)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
                return null;

            // Map non-null properties from DTO to entity
            _mapper.Map(updateBookDto, book);

            // Special handling for title/slug
            if (updateBookDto.Title != null)
            {
                book.slug = GenerateSlug(updateBookDto.Title);
            }

            // Handle author relationships
            if (updateBookDto.AuthorIds?.Any() == true)
            {
                var authors = await _context.Authors
                    .Where(a => updateBookDto.AuthorIds.Contains(a.Id))
                    .ToListAsync();
                if (authors.Count != updateBookDto.AuthorIds.Count)
                    throw new Exception("One or more authors not found");
                book.Authors = authors;
            }

            // Handle publisher relationships
            if (updateBookDto.PublisherIds?.Any() == true)
            {
                var publishers = await _context.Publishers
                    .Where(p => updateBookDto.PublisherIds.Contains(p.Id))
                    .ToListAsync();
                if (publishers.Count != updateBookDto.PublisherIds.Count)
                    throw new Exception("One or more publishers not found");
                book.Publishers = publishers;
            }

            book.LastUpdated = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return _mapper.Map<BookDto>(book);
        }

        public async Task<bool> DeleteBookAsync(Guid id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
                return false;

            // Delete cover image if exists
            if (!string.IsNullOrEmpty(book.ImageUrl))
            {
                try
                {
                    var publicId = ExtractPublicIdFromUrl(book.ImageUrl);
                    await _cloudinaryService.DeleteImageAsync(publicId);
                }
                catch (Exception ex)
                {
                    // Log the error but continue with book deletion
                    Console.WriteLine($"Failed to delete cover image: {ex.Message}");
                }
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateStockAsync(Guid id, int quantity)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
                return false;

            book.StockQuantity = quantity;
            book.LastUpdated = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetDiscountAsync(Guid id, decimal percentage, DateTime? startDate, DateTime? endDate)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
                return false;

            if (percentage < 0 || percentage > 100)
                throw new Exception("Discount percentage must be between 0 and 100");

            book.DiscountPercentage = percentage;
            book.DiscountStartDate = startDate;
            book.DiscountEndDate = endDate;
            book.LastUpdated = DateTime.UtcNow;

            // Update isOnSale based on current discount status
            var now = DateTime.UtcNow;
            book.IsOnSale = percentage > 0 && 
                (!startDate.HasValue || startDate.Value <= now) && 
                (!endDate.HasValue || endDate.Value >= now);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveDiscountAsync(Guid id)
        {
            return await SetDiscountAsync(id, 0, null, null);
        }

        public async Task<double> GetAverageRatingAsync(Guid id)
        {
            return await _context.Reviews
                .Where(r => r.BookId == id)
                .AverageAsync(r => r.Rating);
        }

        public async Task<bool> IsAvailableAsync(Guid id)
        {
            var book = await _context.Books.FindAsync(id);
            return book?.StockQuantity > 0;
        }

        public async Task<bool> UpdateCoverImageAsync(Guid id, IFormFile image)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
                return false;

            try
            {
                // Delete existing image if any
                if (!string.IsNullOrEmpty(book.ImageUrl))
                {
                    var publicId = ExtractPublicIdFromUrl(book.ImageUrl);
                    await _cloudinaryService.DeleteImageAsync(publicId);
                }

                // Upload new image
                var imageUrl = await _cloudinaryService.UploadImageAsync(image);
                book.ImageUrl = imageUrl;
                book.LastUpdated = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update cover image: {ex.Message}");
            }
        }

        public async Task<bool> IsBookmarkedByUserAsync(Guid bookId, Guid userId)
        {
            // Try to find the book first to ensure it exists
            var book = await _context.Books
                .Include(b => b.BookmarkedBy)
                .FirstOrDefaultAsync(b => b.Id == bookId);

            if (book == null)
                throw new Exception($"Book with ID {bookId} not found");

            // Check if the user is in the BookmarkedBy collection
            return book.BookmarkedBy.Any(u => u.Id == userId);
        }

        private static string GenerateSlug(string title)
        {
            // Remove special characters
            string slug = Regex.Replace(title.ToLower(), @"[^a-z0-9\s-]", "");
            
            // Replace spaces with hyphens
            slug = Regex.Replace(slug, @"\s+", "-");
            
            // Remove multiple hyphens
            slug = Regex.Replace(slug, @"-+", "-");
            
            // Trim hyphens from start and end
            slug = slug.Trim('-');
            
            return slug;
        }

        private static string ExtractPublicIdFromUrl(string imageUrl)
        {
            // Example URL: https://res.cloudinary.com/your-cloud-name/image/upload/v1234567890/public-id.jpg
            var match = Regex.Match(imageUrl, @"upload/(?:v\d+/)?(.+)\.\w+$");
            return match.Success ? match.Groups[1].Value : string.Empty;
        }
    }
}