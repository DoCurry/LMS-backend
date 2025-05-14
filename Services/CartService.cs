using AutoMapper;
using Microsoft.EntityFrameworkCore;
using LMS_backend.Data;
using LMS_backend.Dtos;
using LMS_backend.Entities;
using LMS_backend.Services.Interfaces;

namespace LMS_backend.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IOrderService _orderService;

        public CartService(ApplicationDbContext context, IMapper mapper, IOrderService orderService)
        {
            _context = context;
            _mapper = mapper;
            _orderService = orderService;
        }

        public async Task<CartDto?> GetCartAsync(Guid userId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            await LoadCartRelationsAsync(cart);
            return _mapper.Map<CartDto>(cart);
        }

        public async Task<CartDto> AddToCartAsync(Guid userId, AddToCartDto addToCartDto)
        {
            // Start fresh database context for this operation
            _context.ChangeTracker.Clear();

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();

                // Reload cart after creation
                cart = await _context.Carts
                    .Include(c => c.Items)
                    .FirstAsync(c => c.Id == cart.Id);
            }

            var book = await _context.Books.FindAsync(addToCartDto.BookId);
            if (book == null)
                throw new Exception("Book not found");

            if (book.StockQuantity < addToCartDto.Quantity)
                throw new Exception("Insufficient stock");

            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.BookId == addToCartDto.BookId);

            if (existingItem != null)
                throw new Exception("Book is already in cart");            // Calculate price after discount
            var discountedPrice = CalculateDiscountedPrice(book);

            var cartItem = new CartItem
            {
                CartId = cart.Id,
                BookId = book.Id,
                Quantity = addToCartDto.Quantity,
                UnitPrice = discountedPrice,
                Subtotal = discountedPrice * addToCartDto.Quantity,
                CreatedAt = DateTime.UtcNow
            };

            _context.CartItems.Add(cartItem);
            await _context.SaveChangesAsync();

            // Reload the cart with updated data
            cart = await _context.Carts
                .Include(c => c.Items)
                .FirstAsync(c => c.Id == cart.Id);

            cart.TotalAmount = cart.Items.Sum(i => i.Subtotal);
            cart.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await LoadCartRelationsAsync(cart);

            var cartDto = _mapper.Map<CartDto>(cart);
            if (cartDto == null)
                throw new Exception("Failed to map cart data");

            return cartDto;
        }

        public async Task<CartDto?> UpdateCartItemAsync(Guid userId, Guid cartItemId, UpdateCartItemDto updateCartItemDto)
        {
            // Start fresh database context
            _context.ChangeTracker.Clear();

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return null;

            var cartItem = await _context.CartItems
                .Include(ci => ci.Book)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.CartId == cart.Id);

            if (cartItem == null || cartItem.Book == null)
                return null; if (cartItem.Book.StockQuantity < updateCartItemDto.Quantity)
                throw new Exception("Insufficient stock");            // Calculate price after discount
            var discountedPrice = CalculateDiscountedPrice(cartItem.Book);

            cartItem.UnitPrice = discountedPrice;
            cartItem.Quantity = updateCartItemDto.Quantity;
            cartItem.Subtotal = discountedPrice * updateCartItemDto.Quantity;
            cartItem.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Reload cart to get fresh data
            cart = await _context.Carts
                .Include(c => c.Items)
                .FirstAsync(c => c.Id == cart.Id);

            cart.TotalAmount = cart.Items.Sum(i => i.Subtotal);
            cart.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await LoadCartRelationsAsync(cart);

            return _mapper.Map<CartDto>(cart);
        }

        public async Task<bool> RemoveFromCartAsync(Guid userId, Guid cartItemId)
        {
            // Start fresh database context
            _context.ChangeTracker.Clear();

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return false;

            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.CartId == cart.Id);

            if (cartItem == null)
                return false;

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            // Reload cart to get fresh data
            cart = await _context.Carts
                .Include(c => c.Items)
                .FirstAsync(c => c.Id == cart.Id);

            cart.TotalAmount = cart.Items.Sum(i => i.Subtotal);
            cart.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ClearCartAsync(Guid userId)
        {
            // Start fresh database context
            _context.ChangeTracker.Clear();

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return false;

            _context.CartItems.RemoveRange(cart.Items);
            await _context.SaveChangesAsync();

            // Reload cart to get fresh data
            cart = await _context.Carts
                .FirstAsync(c => c.Id == cart.Id);

            cart.TotalAmount = 0;
            cart.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<CartSummaryDto> GetCartSummaryAsync(Guid userId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            await LoadCartRelationsAsync(cart);

            var itemCount = cart.Items.Sum(i => i.Quantity);
            var totalAmount = cart.TotalAmount;

            var discountAmount = await _orderService.CalculateDiscountAsync(userId, totalAmount, itemCount);
            var finalAmount = totalAmount - discountAmount;

            return new CartSummaryDto
            {
                ItemCount = itemCount,
                TotalAmount = totalAmount,
                DiscountAmount = discountAmount,
                FinalAmount = finalAmount
            };
        }

        private async Task LoadCartRelationsAsync(Cart cart)
        {
            await _context.Entry(cart)
                .Collection(c => c.Items)
                .Query()
                .Include(i => i.Book!)
                .ThenInclude(b => b!.Authors)
                .Include(i => i.Book!)
                .ThenInclude(b => b!.Publishers)
                .LoadAsync();

            await _context.Entry(cart)
                .Reference(c => c.User)
                .LoadAsync();
        }

        private async Task<Cart> GetOrCreateCartAsync(Guid userId)
        {
            // Start fresh database context
            _context.ChangeTracker.Clear();

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();

                // Reload cart after creation
                cart = await _context.Carts
                    .Include(c => c.Items)
                    .FirstAsync(c => c.Id == cart.Id);
            }

            return cart;
        }
        private decimal CalculateDiscountedPrice(Book book)
        {
            if (book.DiscountPercentage == null || book.DiscountPercentage <= 0 || book.DiscountPercentage > 100)
                return book.Price;

            var now = DateTime.UtcNow;
            var isDiscountValid = (!book.DiscountStartDate.HasValue || book.DiscountStartDate.Value <= now) &&
                                (!book.DiscountEndDate.HasValue || book.DiscountEndDate.Value >= now);

            if (!isDiscountValid)
                return book.Price;

            var discountAmount = Math.Round(book.Price * (book.DiscountPercentage.Value / 100m), 2);
            return book.Price - discountAmount;
        }
    }
}