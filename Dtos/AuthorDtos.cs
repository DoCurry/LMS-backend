using System.ComponentModel.DataAnnotations;

namespace LMS_backend.Dtos
{
    public class AuthorDto
    {
        public required Guid Id { get; set; }
        public required string Name { get; set; }
        public string? Email { get; set; }
        public required int BookCount { get; set; }
        public required DateTime CreatedAt { get; set; }
        public DateTime? LastUpdated { get; set; }
    }

    public class CreateAuthorDto
    {
        [Required(ErrorMessage = "Author name is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Author name must be between 1 and 100 characters")]
        public required string Name { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(100)]
        public string? Email { get; set; }
    }

    public class UpdateAuthorDto
    {
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Author name must be between 1 and 100 characters")]
        public string? Name { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(100)]
        public string? Email { get; set; }
    }
}