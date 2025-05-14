using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LMS_backend.Entities
{
    [Table("Publisher")]
    [Index(nameof(Name), IsUnique = true)]
    public class Publisher
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "Publisher name is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Publisher name must be between 1 and 100 characters")]
        public required string Name { get; set; }

        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string? Email { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastUpdated { get; set; }

        public virtual ICollection<Book> Books { get; set; } = new HashSet<Book>();
    }
}