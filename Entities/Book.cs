using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LMS_backend.Entities
{
    [Table("Book")]
    [Index(nameof(ISBN), IsUnique = true)]
    [Index(nameof(slug), IsUnique = true)]

    public class Book
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters")]
        public required string Title { get; set; }

        [Required(ErrorMessage = "ISBN is required")]
        [StringLength(13, MinimumLength = 10, ErrorMessage = "ISBN must be between 10 and 13 characters")]
        public required string ISBN { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public required string Description { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        public virtual ICollection<Author> Authors { get; set; } = new HashSet<Author>();
        public virtual ICollection<Publisher> Publishers { get; set; } = new HashSet<Publisher>();

        [Required]
        public DateTime PublicationDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 9999.99, ErrorMessage = "Price must be between 0.01 and 9999.99")]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative")]
        public int StockQuantity { get; set; }

        [Required]
        [StringLength(50)]
        public Language Language { get; set; }

        [Required]
        public BookFormat Format { get; set; }

        [Required]
        [StringLength(50)]
        public Genre Genre { get; set; }

        public bool IsAvailableInLibrary { get; set; }

        [Range(0, int.MaxValue)]
        public int SoldCount { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        [Range(0, 100, ErrorMessage = "Discount percentage must be between 0 and 100")]
        public decimal? DiscountPercentage { get; set; }

        public DateTime? DiscountStartDate { get; set; }

        public DateTime? DiscountEndDate { get; set; }

        public bool? IsOnSale { get; set; }

        public required string slug { get; set; }
        public virtual ICollection<Review> Reviews { get; set; } = new HashSet<Review>();
        public virtual ICollection<User> BookmarkedBy { get; set; } = new HashSet<User>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastUpdated { get; set; }
    }

    public enum BookFormat
    {
        Paperback,
        Hardcover,
        Exclusive
    }

    public enum Language
    {
        English,
        Nepali,
        Hindi,
        Spanish
    }

    public enum Genre
    {
        Action,
        Comedy,
        Drama,
        Horror,
        SciFi,
        Romance
    }
}
