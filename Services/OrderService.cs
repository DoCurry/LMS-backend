using AutoMapper;
using Microsoft.EntityFrameworkCore;
using LMS_backend.Data;
using LMS_backend.Dtos;
using LMS_backend.Entities;
using LMS_backend.Services.Interfaces;

namespace LMS_backend.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IBookService _bookService;
        private readonly IEmailService _emailService;

        public OrderService(ApplicationDbContext context, IMapper mapper, IBookService bookService, IEmailService emailService)
        {
            _context = context;
            _mapper = mapper;
            _bookService = bookService;
            _emailService = emailService;
        }

        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Book!)
                .ThenInclude(b => b!.Authors)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Book!)
                .ThenInclude(b => b!.Publishers)
                .ToListAsync();

            return orders.Select(o => _mapper.Map<OrderDto>(o)!)
                .Where(o => o != null)
                .ToList();
        }

        public async Task<OrderDto?> GetOrderByIdAsync(Guid id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Book!)
                .ThenInclude(b => b!.Authors)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Book!)
                .ThenInclude(b => b!.Publishers)
                .FirstOrDefaultAsync(o => o.Id == id);

            return order == null ? null : _mapper.Map<OrderDto>(order);
        }

        public async Task<IEnumerable<OrderDto>> GetUserOrdersAsync(Guid userId)
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Book!)
                .ThenInclude(b => b!.Authors)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Book!)
                .ThenInclude(b => b!.Publishers)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return orders.Select(o => _mapper.Map<OrderDto>(o)!)
                .Where(o => o != null)
                .ToList();
        }

        public async Task<OrderResponseDto> CreateOrderAsync(Guid userId, CreateOrderDto createOrderDto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new Exception("User not found");

            // Validate items and calculate totals
            decimal subTotal = 0;
            var orderItems = new List<OrderItem>();
            
            foreach (var item in createOrderDto.Items)
            {
                var book = await _context.Books.FindAsync(item.BookId);
                if (book == null)
                    throw new Exception($"Book not found: {item.BookId}");

                if (book.StockQuantity < item.Quantity)
                    throw new Exception($"Insufficient stock for book: {book.Title}");

                var price = book.Price;
                var discount = (book.DiscountPercentage/100)*price ?? 0;
                var now = DateTime.UtcNow;

                // Check if discount is valid
                if (discount > 0)
                {
                    if ((book.DiscountStartDate.HasValue && book.DiscountStartDate.Value > now) ||
                        (book.DiscountEndDate.HasValue && book.DiscountEndDate.Value < now))
                    {
                        discount = 0;
                    } else {
                        price -= discount;
                    }
                }

                var orderItem = new OrderItem
                {
                    BookId = book.Id,
                    Quantity = item.Quantity,
                    PriceAtTime = price,
                    DiscountAtTime = discount
                };

                orderItems.Add(orderItem);
                subTotal += price * item.Quantity;

                // Update stock
                book.StockQuantity -= item.Quantity;
                book.SoldCount += item.Quantity;
            }

            // Calculate discounts
            var discountAmount = await CalculateDiscountAsync(userId, subTotal, orderItems.Sum(i => i.Quantity));
            var finalTotal = subTotal - discountAmount;

            // Create order
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                SubTotal = subTotal,
                DiscountAmount = discountAmount,
                FinalTotal = finalTotal,
                ClaimCode = await GenerateUniqueClaimCodeAsync(),
                OrderItems = orderItems
            };

            _context.Orders.Add(order);

            // Update user's discount eligibility (10% discount after every 10 successful orders)
            var successfulOrderCount = await _context.Orders
                .CountAsync(o => o.UserId == userId && !o.IsCancelled);

            if ((successfulOrderCount + 1) % 10 == 0)
            {
                user.IsDiscountAvailable = true;
            }

            await _context.SaveChangesAsync();

            // Create response with null check
            var mappedOrderItems = order.OrderItems
                .Select(oi => _mapper.Map<OrderItemDto>(oi)!)
                .Where(oi => oi != null)
                .ToList();

            var response = new OrderResponseDto
            {
                Id = order.Id,
                ClaimCode = order.ClaimCode,
                SubTotal = order.SubTotal,
                DiscountAmount = order.DiscountAmount,
                FinalTotal = order.FinalTotal,
                Email = user.Email,
                OrderDate = order.OrderDate,
                Items = mappedOrderItems
            };

            // Send order confirmation email
            await _emailService.SendOrderConfirmationAsync(response);

            return response;
        }

        public async Task<OrderResponseDto> CreateOrderFromCartAsync(Guid userId)
        {
            // Start database transaction
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    throw new Exception("User not found");

                // Get cart with items
                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .ThenInclude(i => i.Book)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null || !cart.Items.Any())
                    throw new Exception("Cart is empty");

                // Validate items and calculate totals
                decimal subTotal = 0;
                var orderItems = new List<OrderItem>();
                
                foreach (var item in cart.Items)
                {
                    var book = item.Book;
                    if (book == null)
                        throw new Exception($"Book not found for cart item");

                    if (book.StockQuantity < item.Quantity)
                        throw new Exception($"Insufficient stock for book: {book.Title}");

                    var orderItem = new OrderItem
                    {
                        BookId = book.Id,
                        Quantity = item.Quantity,
                        PriceAtTime = item.UnitPrice,
                        DiscountAtTime = book.Price - item.UnitPrice,
                        CreatedAt = DateTime.UtcNow
                    };

                    orderItems.Add(orderItem);
                    subTotal += item.Subtotal;

                    // Update stock
                    book.StockQuantity -= item.Quantity;
                    book.SoldCount += item.Quantity;
                    book.LastUpdated = DateTime.UtcNow;
                }

                // Calculate discounts
                var discountAmount = await CalculateDiscountAsync(userId, subTotal, orderItems.Sum(i => i.Quantity));
                var finalTotal = subTotal - discountAmount;

                // Create order
                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatus.Pending,
                    SubTotal = subTotal,
                    DiscountAmount = discountAmount,
                    FinalTotal = finalTotal,
                    ClaimCode = await GenerateUniqueClaimCodeAsync(),
                    OrderItems = orderItems
                };

                _context.Orders.Add(order);

                // Update user's discount eligibility (10% discount after every 10 successful orders)
                var successfulOrderCount = await _context.Orders
                    .CountAsync(o => o.UserId == userId && !o.IsCancelled);

                if ((successfulOrderCount + 1) % 10 == 0)
                {
                    user.IsDiscountAvailable = true;
                    user.LastUpdated = DateTime.UtcNow;
                }

                // Clear the cart
                _context.CartItems.RemoveRange(cart.Items);
                cart.TotalAmount = 0;
                cart.LastUpdated = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var mappedOrderItems = order.OrderItems
                    .Select(oi => _mapper.Map<OrderItemDto>(oi)!)
                    .Where(oi => oi != null)
                    .ToList();

                var response = new OrderResponseDto
                {
                    Id = order.Id,
                    ClaimCode = order.ClaimCode,
                    SubTotal = order.SubTotal,
                    DiscountAmount = order.DiscountAmount,
                    FinalTotal = order.FinalTotal,
                    Email = user.Email,
                    OrderDate = order.OrderDate,
                    Items = mappedOrderItems
                };

                // Send order confirmation email
                await _emailService.SendOrderConfirmationAsync(response);

                // Commit transaction
                await transaction.CommitAsync();

                return response;
            }
            catch (Exception)
            {
                // Rollback transaction on error
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> CancelOrderAsync(Guid orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Book)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null || order.IsCancelled)
                return false;

            if (order.Status == OrderStatus.Completed)
                throw new Exception("Cannot cancel a completed order");

            // Restore stock with null check
            foreach (var item in order.OrderItems.Where(i => i.Book != null))
            {
                item.Book!.StockQuantity += item.Quantity;
                item.Book.SoldCount -= item.Quantity;
            }

            order.IsCancelled = true;
            order.Status = OrderStatus.Cancelled;
            order.CancellationDate = DateTime.UtcNow;
            order.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Send cancellation email
            var orderDto = _mapper.Map<OrderDto>(order);
            if (orderDto != null)
            {
                await _emailService.SendOrderCancellationAsync(orderDto);
            }

            return true;
        }        public async Task<bool> CompleteOrderAsync(Guid orderId, string membershipId)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return false;

            if (order.IsCancelled)
                throw new Exception("Cannot complete a cancelled order");

            if (order.Status == OrderStatus.Completed)
                throw new Exception("Order is already completed");

            if (order.Status != OrderStatus.ReadyForPickup)
                throw new Exception("Order must be ready for pickup before it can be completed");

            if (order.User?.MembershipId != membershipId)
                throw new Exception("Invalid membership ID");

            order.Status = OrderStatus.Completed;
            order.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Send completion email
            var orderDto = _mapper.Map<OrderDto>(order);
            if (orderDto != null)
            {
                await _emailService.SendOrderReadyForPickupAsync(orderDto);
            }

            return true;
        }

        public async Task<decimal> CalculateDiscountAsync(Guid userId, decimal subTotal, int itemCount)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new Exception("User not found");

            if (subTotal <= 0)
                return 0;

            decimal discountAmount = 0;
            decimal remainingSubTotal = subTotal;

            // Apply 5% discount for orders with 5 or more items
            if (itemCount >= 5)
            {
                var bulkDiscount = subTotal * 0.05m;
                discountAmount += bulkDiscount;
                remainingSubTotal -= bulkDiscount;
            }

            // Apply stackable 10% loyalty discount if available
            if (user.IsDiscountAvailable)
            {
                var loyaltyDiscount = remainingSubTotal * 0.10m;
                discountAmount += loyaltyDiscount;
                user.IsDiscountAvailable = false; // Reset the discount flag
                await _context.SaveChangesAsync(); // Save the user discount status change
            }

            // Ensure total discount doesn't exceed subtotal
            return Math.Min(discountAmount, subTotal);
        }

        public async Task<bool> ValidateClaimCodeAsync(Guid orderId, string claimCode)
        {
            var order = await _context.Orders.FindAsync(orderId);
            return order?.ClaimCode == claimCode;
        }

        public async Task<OrderDto?> GetOrderByClaimCodeAsync(string claimCode)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Book!)
                .ThenInclude(b => b!.Authors)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Book!)
                .ThenInclude(b => b!.Publishers)
                .FirstOrDefaultAsync(o => o.ClaimCode == claimCode);

            return order == null ? null : _mapper.Map<OrderDto>(order);
        }

        private async Task<string> GenerateUniqueClaimCodeAsync()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            string claimCode;
            bool isUnique;

            do
            {
                claimCode = new string(Enumerable.Repeat(chars, 8)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
                isUnique = !await _context.Orders.AnyAsync(o => o.ClaimCode == claimCode);
            } while (!isUnique);

            return claimCode;
        }

        public async Task<bool> SetOrderReadyForPickupAsync(Guid orderId)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return false;

            if (order.Status != OrderStatus.Pending)
                throw new Exception($"Cannot mark order as ready for pickup. Current status: {order.Status}");

            if (order.IsCancelled)
                throw new Exception("Cannot mark cancelled order as ready for pickup");

            order.Status = OrderStatus.ReadyForPickup;
            order.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Send ready for pickup email
            var orderDto = _mapper.Map<OrderDto>(order);
            if (orderDto != null)
            {
                await _emailService.SendOrderReadyForPickupAsync(orderDto);
            }

            return true;
        }

        public async Task<bool> CompleteClaimedOrderAsync(Guid orderId, string claimCode, string membershipId)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.ClaimCode == claimCode);

            if (order == null)
                throw new Exception($"Order with claim code {claimCode} not found");

            if (order.IsCancelled)
                throw new Exception("Cannot complete a cancelled order");

            if (order.Status == OrderStatus.Completed)
                throw new Exception("Order is already completed");

            if (order.Status != OrderStatus.ReadyForPickup)
                throw new Exception("Order must be ready for pickup before it can be completed");

            if (order.User?.MembershipId != membershipId)
                throw new Exception("Invalid membership ID");

            order.Status = OrderStatus.Completed;
            order.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var orderDto = _mapper.Map<OrderDto>(order);
            if (orderDto != null)
            {
                await _emailService.SendOrderReadyForPickupAsync(orderDto);
            }

            return true;
        }
    }
}