using System.ComponentModel.DataAnnotations;
using LMS_backend.Entities;

namespace LMS_backend.Dtos
{
    public class OrderDto
    {
        public required Guid Id { get; set; }
        public required string ClaimCode { get; set; }
        public required Guid UserId { get; set; }
        public required UserDto User { get; set; }
        public required DateTime OrderDate { get; set; }
        public required OrderStatus Status { get; set; }
        public required decimal SubTotal { get; set; }
        public required decimal DiscountAmount { get; set; }
        public required decimal FinalTotal { get; set; }
        public required bool IsCancelled { get; set; }
        public DateTime? CancellationDate { get; set; }
        public required ICollection<OrderItemDto> OrderItems { get; set; }
        public required DateTime CreatedAt { get; set; }
        public DateTime? LastUpdated { get; set; }
    }

    public class OrderItemDto
    {
        public required Guid Id { get; set; }
        public required Guid BookId { get; set; }
        public required BookDto Book { get; set; }
        public required int Quantity { get; set; }
        public required decimal PriceAtTime { get; set; }
        public decimal? DiscountAtTime { get; set; }
        public required DateTime CreatedAt { get; set; }
        public DateTime? LastUpdated { get; set; }
    }

    public class CreateOrderDto
    {
        [Required]
        public required ICollection<OrderItemCreateDto> Items { get; set; }
    }

    public class OrderItemCreateDto
    {
        [Required]
        public required Guid BookId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public required int Quantity { get; set; }
    }

    public class OrderResponseDto
    {
        public required Guid Id { get; set; }
        public required string ClaimCode { get; set; }
        public required decimal SubTotal { get; set; }
        public required decimal DiscountAmount { get; set; }
        public required decimal FinalTotal { get; set; }
        public required string Email { get; set; }
        public required DateTime OrderDate { get; set; }
        public required ICollection<OrderItemDto> Items { get; set; }
    }
}