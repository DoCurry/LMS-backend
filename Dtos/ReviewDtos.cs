using System.ComponentModel.DataAnnotations;

namespace LMS_backend.Dtos
{
    public class ReviewDto
    {
        public required Guid Id { get; set; }
        public required Guid BookId { get; set; }
        public required BookDto Book { get; set; }
        public required Guid UserId { get; set; }
        public required UserDto User { get; set; }
        public required int Rating { get; set; }
        public string? Comment { get; set; }
        public required DateTime CreatedAt { get; set; }
    }

    public class CreateReviewDto
    {
        [Required]
        public required Guid BookId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public required int Rating { get; set; }

        [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
        public string? Comment { get; set; }
    }

    public class UpdateReviewDto
    {
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int? Rating { get; set; }

        [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
        public string? Comment { get; set; }
    }
}