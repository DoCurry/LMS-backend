using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LMS_backend.Dtos;
using LMS_backend.Services.Interfaces;

namespace LMS_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnnouncementController : ControllerBase
    {
        private readonly IAnnouncementService _announcementService;

        public AnnouncementController(IAnnouncementService announcementService)
        {
            _announcementService = announcementService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AnnouncementDto>>> GetAllAnnouncements()
        {
            var announcements = await _announcementService.GetAllAnnouncementsAsync();
            return Ok(new { message = "Announcements retrieved successfully", data = announcements });
        }

        [HttpGet("active")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AnnouncementDto>>> GetActiveAnnouncements()
        {
            var announcements = await _announcementService.GetActiveAnnouncementsAsync();
            return Ok(new { message = "Active announcements retrieved successfully", data = announcements });
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AnnouncementDto>> GetAnnouncement(Guid id)
        {
            var announcement = await _announcementService.GetAnnouncementByIdAsync(id);
            if (announcement == null)
                return NotFound(new { message = $"Announcement with ID {id} not found" });
            return Ok(new { message = "Announcement retrieved successfully", data = announcement });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<AnnouncementDto>> CreateAnnouncement(CreateAnnouncementDto createAnnouncementDto)
        {
            try
            {
                var announcement = await _announcementService.CreateAnnouncementAsync(createAnnouncementDto);
                return CreatedAtAction(
                    nameof(GetAnnouncement), 
                    new { id = announcement.Id }, 
                    new { message = "Announcement created successfully", data = announcement }
                );
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to create announcement", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<AnnouncementDto>> UpdateAnnouncement(Guid id, UpdateAnnouncementDto updateAnnouncementDto)
        {
            try
            {
                var announcement = await _announcementService.UpdateAnnouncementAsync(id, updateAnnouncementDto);
                if (announcement == null)
                    return NotFound(new { message = $"Announcement with ID {id} not found" });
                return Ok(new { message = "Announcement updated successfully", data = announcement });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to update announcement", error = ex.Message });
            }
        }        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> DeleteAnnouncement(Guid id)
        {            var result = await _announcementService.DeleteAnnouncementAsync(id);
            if (!result)
                return NotFound(new { message = $"Announcement with ID {id} not found" });
            return Ok(new { message = "Announcement deleted successfully" });
        }        [HttpPatch("{id}/toggle")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> ToggleAnnouncementStatus(Guid id)
        {            var result = await _announcementService.ToggleAnnouncementStatusAsync(id);
            if (!result)
                return NotFound(new { message = $"Announcement with ID {id} not found" });
            return Ok(new { message = "Announcement status toggled successfully" });
        }
    }
}