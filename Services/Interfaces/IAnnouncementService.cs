using LMS_backend.Dtos;

namespace LMS_backend.Services.Interfaces
{
    public interface IAnnouncementService
    {
        Task<IEnumerable<AnnouncementDto>> GetAllAnnouncementsAsync();
        Task<IEnumerable<AnnouncementDto>> GetActiveAnnouncementsAsync();
        Task<AnnouncementDto?> GetAnnouncementByIdAsync(Guid id);
        Task<AnnouncementDto> CreateAnnouncementAsync(CreateAnnouncementDto createAnnouncementDto);
        Task<AnnouncementDto?> UpdateAnnouncementAsync(Guid id, UpdateAnnouncementDto updateAnnouncementDto);
        Task<bool> DeleteAnnouncementAsync(Guid id);
        Task<bool> ToggleAnnouncementStatusAsync(Guid id);
    }
}