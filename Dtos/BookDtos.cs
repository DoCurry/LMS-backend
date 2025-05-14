using System.ComponentModel.DataAnnotations;
using LMS_backend.Entities;
using Microsoft.AspNetCore.Http;

namespace LMS_backend.Dtos
{
    public class BookDto
    {
        public required Guid Id { get; set; }
        public required string Title { get; set; }
        public required string ISBN { get; set; }
        public required string Description { get; set; }
        public string? ImageUrl { get; set; }
        public required ICollection<AuthorDto> Authors { get; set; }
        public required ICollection<PublisherDto> Publishers { get; set; }
        public required DateTime PublicationDate { get; set; }
        public required decimal Price { get; set; }
        public required int StockQuantity { get; set; }
        public required Language Language { get; set; }
        public required BookFormat Format { get; set; }
        public required Genre Genre { get; set; }
        public required bool IsAvailableInLibrary { get; set; }
        public required int SoldCount { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public DateTime? DiscountStartDate { get; set; }
        public DateTime? DiscountEndDate { get; set; }
        public required bool IsOnSale { get; set; }
        public required string Slug { get; set; }
        public required double AverageRating { get; set; }
        public required int ReviewCount { get; set; }
        public required DateTime CreatedAt { get; set; }
        public DateTime? LastUpdated { get; set; }
    }

    public class CreateBookDto
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters")]
        public required string Title { get; set; }

        [Required(ErrorMessage = "ISBN is required")]
        [StringLength(13, MinimumLength = 10, ErrorMessage = "ISBN must be between 10 and 13 characters")]
        public required string ISBN { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public required string Description { get; set; }

        [Required(ErrorMessage = "At least one author is required")]
        public required ICollection<Guid> AuthorIds { get; set; }

        [Required(ErrorMessage = "At least one publisher is required")]
        public required ICollection<Guid> PublisherIds { get; set; }

        [Required(ErrorMessage = "Publication date is required")]
        public required DateTime PublicationDate { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 9999.99, ErrorMessage = "Price must be between 0.01 and 9999.99")]
        public required decimal Price { get; set; }

        [Required(ErrorMessage = "Stock quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative")]
        public required int StockQuantity { get; set; }

        [Required(ErrorMessage = "Language is required")]
        public required Language Language { get; set; }

        [Required(ErrorMessage = "Format is required")]
        public required BookFormat Format { get; set; }

        [Required(ErrorMessage = "Genre is required")]
        public required Genre Genre { get; set; }

        public bool IsAvailableInLibrary { get; set; }

        public IFormFile? CoverImage { get; set; }

    }

    public class UpdateBookDto
    {
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters")]
        public string? Title { get; set; }

        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string? Description { get; set; }

        public ICollection<Guid>? AuthorIds { get; set; }
        public ICollection<Guid>? PublisherIds { get; set; }

        [Range(0.01, 9999.99, ErrorMessage = "Price must be between 0.01 and 9999.99")]
        public decimal? Price { get; set; }

        public Language? Language { get; set; }
        public BookFormat? Format { get; set; }
        public Genre? Genre { get; set; }
        public bool? IsAvailableInLibrary { get; set; }
    }

    public class BookFilterDto
    {
        public string? SearchTerm { get; set; }
        public ICollection<Guid>? AuthorIds { get; set; }
        public ICollection<Guid>? PublisherIds { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public ICollection<Genre>? Genres { get; set; }
        public ICollection<Language>? Languages { get; set; }
        public ICollection<BookFormat>? Formats { get; set; }
        public bool? IsAvailableInLibrary { get; set; }
        public bool? HasDiscount { get; set; }
        public int? MinRating { get; set; }
        public string? SortBy { get; set; }
        public bool? SortDescending { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}