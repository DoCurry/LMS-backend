using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LMS_backend.Dtos;
using LMS_backend.Services.Interfaces;
using System.Text.Json;
using System.Security.Claims;

namespace LMS_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookController : ControllerBase 
    {
        private readonly IBookService _bookService;

        public BookController(IBookService bookService)
        {
            _bookService = bookService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetAllBooks()
        {
            var books = await _bookService.GetAllBooksAsync();
            return Ok(new { message = "Books retrieved successfully", data = books });
        }

        [HttpGet("filter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetFilteredBooks([FromQuery] BookFilterDto filter)
        {
            var books = await _bookService.GetBooksByFilterAsync(filter);
            return Ok(new { message = "Filtered books retrieved successfully", data = books });
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BookDto>> GetBook(Guid id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null)
                return NotFound(new { message = $"Book with ID {id} not found" });
            return Ok(new { message = "Book retrieved successfully", data = book });
        }

        [HttpGet("isbn/{isbn}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BookDto>> GetBookByISBN(string isbn)
        {
            var book = await _bookService.GetBookByISBNAsync(isbn);
            if (book == null)
                return NotFound(new { message = $"Book with ISBN {isbn} not found" });
            return Ok(new { message = "Book retrieved successfully", data = book });
        }

        [HttpGet("slug/{slug}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BookDto>> GetBookBySlug(string slug)
        {
            var book = await _bookService.GetBookBySlugAsync(slug);
            if (book == null)
                return NotFound(new { message = $"Book with slug {slug} not found" });
            return Ok(new { message = "Book retrieved successfully", data = book });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<BookDto>> CreateBook([FromForm] string bookData, IFormFile? coverImage)
        {
            try
            {
                var createBookDto = JsonSerializer.Deserialize<CreateBookDto>(bookData, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (createBookDto == null)
                    return BadRequest(new { message = "Invalid book data provided" });

                createBookDto.CoverImage = coverImage;
                
                var book = await _bookService.CreateBookAsync(createBookDto);
                return CreatedAtAction(
                    nameof(GetBook), 
                    new { id = book.Id }, 
                    new { message = "Book created successfully", data = book }
                );
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to create book", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<BookDto>> UpdateBook(Guid id, UpdateBookDto updateBookDto)
        {
            try
            {
                var book = await _bookService.UpdateBookAsync(id, updateBookDto);
                if (book == null)
                    return NotFound(new { message = $"Book with ID {id} not found" });
                return Ok(new { message = "Book updated successfully", data = book });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to update book", error = ex.Message });
            }
        }        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> DeleteBook(Guid id)
        {            var result = await _bookService.DeleteBookAsync(id);
            if (!result)
                return NotFound(new { message = $"Book with ID {id} not found" });
            return Ok(new { message = "Book deleted successfully" });
        }

        [HttpGet("best-sellers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetBestSellers([FromQuery] int limit = 10)
        {
            var books = await _bookService.GetBestSellersAsync(limit);
            return Ok(new { message = "Best sellers retrieved successfully", data = books });
        }

        [HttpGet("new-releases")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetNewReleases()
        {
            var books = await _bookService.GetNewReleasesAsync();
            return Ok(new { message = "New releases retrieved successfully", data = books });
        }

        [HttpGet("new-arrivals")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetNewArrivals()
        {
            var books = await _bookService.GetNewArrivalsAsync();
            return Ok(new { message = "New arrivals retrieved successfully", data = books });
        }

        [HttpGet("coming-soon")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetComingSoon()
        {
            var books = await _bookService.GetComingSoonAsync();
            return Ok(new { message = "Coming soon books retrieved successfully", data = books });
        }

        [HttpGet("deals")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetDeals()
        {
            var books = await _bookService.GetDealsAsync();
            return Ok(new { message = "Books with deals retrieved successfully", data = books });
        }

        [HttpGet("{id}/average-rating")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<double>> GetAverageRating(Guid id)
        {
            var rating = await _bookService.GetAverageRatingAsync(id);
            return Ok(new { message = "Average rating retrieved successfully", data = rating });
        }

        [HttpGet("{id}/availability")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<bool>> CheckAvailability(Guid id)
        {
            var isAvailable = await _bookService.IsAvailableAsync(id);
            return Ok(new { message = "Availability status retrieved successfully", data = isAvailable });
        }        [HttpPatch("{id}/stock")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> UpdateStock(Guid id, [FromBody] int quantity)
        {
            var result = await _bookService.UpdateStockAsync(id, quantity);
            if (!result)
                return NotFound(new { message = $"Book with ID {id} not found" });
            return Ok(new { message = "Book stock updated successfully" });
        }        [HttpPatch("{id}/discount")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> SetDiscount(Guid id, [FromQuery] decimal percentage, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _bookService.SetDiscountAsync(id, percentage, startDate, endDate);
                if (!result)
                    return NotFound(new { message = $"Book with ID {id} not found" });
                return Ok(new { message = "Book discount set successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to set discount", error = ex.Message });
            }
        }        [HttpDelete("{id}/discount")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> RemoveDiscount(Guid id)
        {            var result = await _bookService.RemoveDiscountAsync(id);
            if (!result)
                return NotFound(new { message = $"Book with ID {id} not found" });
            return Ok(new { message = "Book discount removed successfully" });
        }

        [HttpPost("{id}/cover-image")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> UploadCoverImage(Guid id, IFormFile image)
        {
            if (image == null || image.Length == 0)
                return BadRequest(new { message = "No image file provided" });

            if (!image.ContentType.StartsWith("image/"))
                return BadRequest(new { message = "File must be an image" });

            var success = await _bookService.UpdateCoverImageAsync(id, image);
            if (!success)
                return NotFound(new { message = $"Book with ID {id} not found" });

            return Ok(new { message = "Cover image uploaded successfully" });
        }

        [HttpGet("{id}/is-bookmarked")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<bool>> IsBookmarkedByCurrentUser(Guid id)
        {
            try
        {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await _bookService.IsBookmarkedByUserAsync(id, userId);
                return Ok(new { message = "Bookmark status retrieved successfully", data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to check bookmark status", error = ex.Message });
            }
        }
    }
}