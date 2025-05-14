using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LMS_backend.Dtos;
using LMS_backend.Services.Interfaces;

namespace LMS_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorController : ControllerBase
    {
        private readonly IAuthorService _authorService;

        public AuthorController(IAuthorService authorService)
        {
            _authorService = authorService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AuthorDto>>> GetAllAuthors()
        {
            var authors = await _authorService.GetAllAuthorsAsync();
            return Ok(new { message = "Authors retrieved successfully", data = authors });
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AuthorDto>> GetAuthor(Guid id)
        {
            var author = await _authorService.GetAuthorByIdAsync(id);
            if (author == null)
                return NotFound(new { message = $"Author with ID {id} not found" });
            return Ok(new { message = "Author retrieved successfully", data = author });
        }

        [HttpGet("{id}/books")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetAuthorBooks(Guid id)
        {
            try
            {
                var books = await _authorService.GetAuthorBooksAsync(id);
                return Ok(new { message = "Author's books retrieved successfully", data = books });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = $"Author with ID {id} not found or has no books", error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<AuthorDto>> CreateAuthor(CreateAuthorDto createAuthorDto)
        {
            try
            {
                var author = await _authorService.CreateAuthorAsync(createAuthorDto);
                return CreatedAtAction(
                    nameof(GetAuthor), 
                    new { id = author.Id }, 
                    new { message = "Author created successfully", data = author }
                );
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to create author", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<AuthorDto>> UpdateAuthor(Guid id, UpdateAuthorDto updateAuthorDto)
        {
            try
            {
                var author = await _authorService.UpdateAuthorAsync(id, updateAuthorDto);
                if (author == null)
                    return NotFound(new { message = $"Author with ID {id} not found" });
                return Ok(new { message = "Author updated successfully", data = author });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to update author", error = ex.Message });
            }
        }        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> DeleteAuthor(Guid id)
        {
            try
            {            var result = await _authorService.DeleteAuthorAsync(id);
                if (!result)
                    return NotFound(new { message = $"Author with ID {id} not found" });
                return Ok(new { message = "Author deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to delete author", error = ex.Message });
            }
        }
    }
}