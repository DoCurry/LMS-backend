using AutoMapper;
using LMS_backend.Dtos;
using LMS_backend.Entities;

namespace LMS_backend.Data
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {

            // User mappings
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.OrderCount,
                    opt => opt.MapFrom(src => src.Orders != null ? src.Orders.Count : 0))
                .ForMember(dest => dest.ReviewCount,
                    opt => opt.MapFrom(src => src.Reviews != null ? src.Reviews.Count : 0))
                .ForMember(dest => dest.BookmarkCount,
                    opt => opt.MapFrom(src => src.Bookmarks != null ? src.Bookmarks.Count : 0));

            CreateMap<RegisterDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => UserRole.Member))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.IsDiscountAvailable, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Orders, opt => opt.Ignore())
                .ForMember(dest => dest.Reviews, opt => opt.Ignore())
                .ForMember(dest => dest.Bookmarks, opt => opt.Ignore());

            CreateMap<UpdateUserDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdated, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<User, AuthResponseDto>()
                .ForMember(dest => dest.Token, opt => opt.Ignore())
                .ForMember(dest => dest.RefreshToken, opt => opt.Ignore())
                .ForMember(dest => dest.ExpiresAt, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src));

            // Book mappings
            CreateMap<Book, BookDto>()
                .ForMember(dest => dest.AverageRating,
                    opt => opt.MapFrom(src => src.Reviews != null && src.Reviews.Any()
                        ? src.Reviews.Average(r => r.Rating)
                        : 0))
                .ForMember(dest => dest.ReviewCount,
                    opt => opt.MapFrom(src => src.Reviews != null ? src.Reviews.Count : 0))
                .ForMember(dest => dest.Slug,
                    opt => opt.MapFrom(src => src.slug));

            CreateMap<CreateBookDto, Book>()
                .ForMember(dest => dest.Authors, opt => opt.Ignore())
                .ForMember(dest => dest.Publishers, opt => opt.Ignore())
                .ForMember(dest => dest.Reviews, opt => opt.Ignore())
                .ForMember(dest => dest.BookmarkedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<UpdateBookDto, Book>()
                .ForMember(dest => dest.Authors, opt => opt.Ignore())
                .ForMember(dest => dest.Publishers, opt => opt.Ignore())
                .ForMember(dest => dest.Reviews, opt => opt.Ignore())
                .ForMember(dest => dest.BookmarkedBy, opt => opt.Ignore())
                .ForMember(dest => dest.slug, opt => opt.Ignore()) // Handle slug separately in service
                .ForMember(dest => dest.LastUpdated, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Author mappings
            CreateMap<Author, AuthorDto>()
                .ForMember(dest => dest.BookCount,
                    opt => opt.MapFrom(src => src.Books != null ? src.Books.Count : 0));

            CreateMap<CreateAuthorDto, Author>()
                .ForMember(dest => dest.Books, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<UpdateAuthorDto, Author>()
                .ForMember(dest => dest.LastUpdated, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Publisher mappings
            CreateMap<Publisher, PublisherDto>()
                .ForMember(dest => dest.BookCount,
                    opt => opt.MapFrom(src => src.Books != null ? src.Books.Count : 0));

            CreateMap<CreatePublisherDto, Publisher>()
                .ForMember(dest => dest.Books, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<UpdatePublisherDto, Publisher>()
                .ForMember(dest => dest.LastUpdated, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Order mappings
            CreateMap<Order, OrderDto>();
            CreateMap<OrderItem, OrderItemDto>();
            CreateMap<OrderItem, OrderResponseDto>()
                .ForMember(dest => dest.Items,
                    opt => opt.MapFrom(src => src.Order != null ? src.Order.OrderItems : Array.Empty<OrderItem>()));

            // Cart mappings
            CreateMap<Cart, CartDto>();
            CreateMap<CartItem, CartItemDto>();
            CreateMap<AddToCartDto, CartItem>();
            CreateMap<UpdateCartItemDto, CartItem>();

            // Review mappings
            CreateMap<Review, ReviewDto>();
            CreateMap<CreateReviewDto, Review>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
            CreateMap<UpdateReviewDto, Review>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Announcement mappings
            CreateMap<Announcement, AnnouncementDto>();
            CreateMap<CreateAnnouncementDto, Announcement>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
            CreateMap<UpdateAnnouncementDto, Announcement>()
                .ForMember(dest => dest.LastUpdated, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}