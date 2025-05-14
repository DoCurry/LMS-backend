using System.ComponentModel.DataAnnotations;
using LMS_backend.Entities;

namespace LMS_backend.Dtos
{
    public class AnnouncementDto
    {
        public required Guid Id { get; set; }
        public required string Title { get; set; }
        public required string Content { get; set; }
        public required DateTime StartDate { get; set; }
        public required DateTime EndDate { get; set; }
        public required AnnouncementType Type { get; set; }
        public required bool IsActive { get; set; }
        public required DateTime CreatedAt { get; set; }
        public DateTime? LastUpdated { get; set; }
    }

    public class CreateAnnouncementDto
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters")]
        public required string Title { get; set; }

        [Required(ErrorMessage = "Content is required")]
        [StringLength(2000, ErrorMessage = "Content cannot exceed 2000 characters")]
        public required string Content { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        public required DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        public required DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Announcement type is required")]
        public required AnnouncementType Type { get; set; }
    }

    public class UpdateAnnouncementDto
    {
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters")]
        public string? Title { get; set; }

        [StringLength(2000, ErrorMessage = "Content cannot exceed 2000 characters")]
        public string? Content { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public AnnouncementType? Type { get; set; }
        public bool? IsActive { get; set; }
    }
}