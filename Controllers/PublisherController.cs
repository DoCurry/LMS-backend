using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LMS_backend.Dtos;
using LMS_backend.Services.Interfaces;

namespace LMS_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PublisherController : ControllerBase
    {
        private readonly IPublisherService _publisherService;

        public PublisherController(IPublisherService publisherService)
        {
            _publisherService = publisherService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PublisherDto>>> GetAllPublishers()
        {
            var publishers = await _publisherService.GetAllPublishersAsync();
            return Ok(new { message = "Publishers retrieved successfully", data = publishers });
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PublisherDto>> GetPublisher(Guid id)
        {
            var publisher = await _publisherService.GetPublisherByIdAsync(id);
            if (publisher == null)
                return NotFound(new { message = $"Publisher with ID {id} not found" });
            return Ok(new { message = "Publisher retrieved successfully", data = publisher });
        }

        [HttpGet("{id}/books")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetPublisherBooks(Guid id)
        {
            try
            {
                var books = await _publisherService.GetPublisherBooksAsync(id);
                return Ok(new { message = "Publisher's books retrieved successfully", data = books });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = $"Publisher with ID {id} not found or has no books", error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PublisherDto>> CreatePublisher(CreatePublisherDto createPublisherDto)
        {
            try
            {
                var publisher = await _publisherService.CreatePublisherAsync(createPublisherDto);
                return CreatedAtAction(
                    nameof(GetPublisher), 
                    new { id = publisher.Id }, 
                    new { message = "Publisher created successfully", data = publisher }
                );
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to create publisher", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PublisherDto>> UpdatePublisher(Guid id, UpdatePublisherDto updatePublisherDto)
        {
            try
            {
                var publisher = await _publisherService.UpdatePublisherAsync(id, updatePublisherDto);
                if (publisher == null)
                    return NotFound(new { message = $"Publisher with ID {id} not found" });
                return Ok(new { message = "Publisher updated successfully", data = publisher });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to update publisher", error = ex.Message });
            }
        }        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> DeletePublisher(Guid id)
        {            try
            {
                var result = await _publisherService.DeletePublisherAsync(id);
                if (!result)
                    return NotFound(new { message = $"Publisher with ID {id} not found" });
                return Ok(new { message = "Publisher deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to delete publisher", error = ex.Message });
            }
        }
    }
}