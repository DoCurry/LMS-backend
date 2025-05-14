using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LMS_backend.Dtos;
using LMS_backend.Services.Interfaces;

namespace LMS_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetAllReviews()
        {
            var reviews = await _reviewService.GetAllReviewsAsync();
            return Ok(new { message = "Reviews retrieved successfully", data = reviews });
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ReviewDto>> GetReview(Guid id)
        {
            var review = await _reviewService.GetReviewByIdAsync(id);
            if (review == null)
                return NotFound(new { message = $"Review with ID {id} not found" });
            return Ok(new { message = "Review retrieved successfully", data = review });
        }

        [HttpGet("book/{bookId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetBookReviews(Guid bookId)
        {
            try
            {
                var reviews = await _reviewService.GetBookReviewsAsync(bookId);
                return Ok(new { message = "Book reviews retrieved successfully", data = reviews });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = $"Book with ID {bookId} not found or has no reviews", error = ex.Message });
            }
        }

        [HttpGet("user/{userId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetUserReviews(Guid userId)
        {
            try
            {
                var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var isAdmin = User.IsInRole("Admin");

                if (!isAdmin && currentUserId != userId)
                    return Forbid();

                var reviews = await _reviewService.GetUserReviewsAsync(userId);
                return Ok(new { message = "User reviews retrieved successfully", data = reviews });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = $"User with ID {userId} not found or has no reviews", error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ReviewDto>> CreateReview(CreateReviewDto createReviewDto)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                
                // Check if user has already reviewed this book
                if (await _reviewService.HasUserReviewedBookAsync(userId, createReviewDto.BookId))
                    return BadRequest(new { message = "You have already reviewed this book" });

                var review = await _reviewService.CreateReviewAsync(userId, createReviewDto);
                return CreatedAtAction(
                    nameof(GetReview), 
                    new { id = review.Id }, 
                    new { message = "Review created successfully", data = review }
                );
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to create review", error = ex.Message });
            }
        }

        [HttpPut("{reviewId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ReviewDto>> UpdateReview(Guid reviewId, UpdateReviewDto updateReviewDto)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var isAdmin = User.IsInRole("Admin");
                var review = await _reviewService.GetReviewByIdAsync(reviewId);

                if (review == null)
                    return NotFound(new { message = $"Review with ID {reviewId} not found" });

                // Only allow review owner or admin to update the review
                if (review.User.Id != userId && !isAdmin)
                    return Forbid();

                var updatedReview = await _reviewService.UpdateReviewAsync(userId, reviewId, updateReviewDto);
                return Ok(new { message = "Review updated successfully", data = updatedReview });
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("not authorized"))
                    return Forbid();
                return BadRequest(new { message = "Failed to update review", error = ex.Message });
            }
        }

        [HttpDelete("{reviewId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteReview(Guid reviewId)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var isAdmin = User.IsInRole("Admin");
                var review = await _reviewService.GetReviewByIdAsync(reviewId);

                if (review == null)
                    return NotFound(new { message = $"Review with ID {reviewId} not found" });

                // Only allow review owner or admin to delete the review
                if (review.User.Id != userId && !isAdmin)
                    return Forbid();

                var result = await _reviewService.DeleteReviewAsync(userId, reviewId);                if (!result)
                    return NotFound(new { message = $"Review with ID {reviewId} not found" });
                return Ok(new { message = "Review deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to delete review", error = ex.Message });
            }
        }
    }
}