using LMS_backend.Dtos;

namespace LMS_backend.Services.Interfaces
{
    public interface IPublisherService
    {
        Task<IEnumerable<PublisherDto>> GetAllPublishersAsync();
        Task<PublisherDto?> GetPublisherByIdAsync(Guid id);
        Task<PublisherDto> CreatePublisherAsync(CreatePublisherDto createPublisherDto);
        Task<PublisherDto?> UpdatePublisherAsync(Guid id, UpdatePublisherDto updatePublisherDto);
        Task<bool> DeletePublisherAsync(Guid id);
        Task<IEnumerable<BookDto>> GetPublisherBooksAsync(Guid id);
    }
}