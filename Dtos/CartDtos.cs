using System.ComponentModel.DataAnnotations;

namespace LMS_backend.Dtos
{
    public class CartDto
    {
        public required Guid Id { get; set; }
        public required Guid UserId { get; set; }
        public required UserDto User { get; set; }
        public required decimal TotalAmount { get; set; }
        public required ICollection<CartItemDto> Items { get; set; }
        public required DateTime CreatedAt { get; set; }
        public DateTime? LastUpdated { get; set; }
    }

    public class CartItemDto
    {
        public required Guid Id { get; set; }
        public required Guid BookId { get; set; }
        public required BookDto Book { get; set; }
        public required int Quantity { get; set; }
        public required decimal UnitPrice { get; set; }
        public required decimal Subtotal { get; set; }
        public required DateTime CreatedAt { get; set; }
        public DateTime? LastUpdated { get; set; }
    }

    public class AddToCartDto
    {
        [Required]
        public required Guid BookId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public required int Quantity { get; set; }
    }

    public class UpdateCartItemDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public required int Quantity { get; set; }
    }

    public class CartSummaryDto
    {
        public required int ItemCount { get; set; }
        public required decimal TotalAmount { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? FinalAmount { get; set; }
    }
}