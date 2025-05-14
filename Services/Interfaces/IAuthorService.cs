using LMS_backend.Dtos;

namespace LMS_backend.Services.Interfaces
{
    public interface IAuthorService
    {
        Task<IEnumerable<AuthorDto>> GetAllAuthorsAsync();
        Task<AuthorDto?> GetAuthorByIdAsync(Guid id);
        Task<AuthorDto> CreateAuthorAsync(CreateAuthorDto createAuthorDto);
        Task<AuthorDto?> UpdateAuthorAsync(Guid id, UpdateAuthorDto updateAuthorDto);
        Task<bool> DeleteAuthorAsync(Guid id);
        Task<IEnumerable<BookDto>> GetAuthorBooksAsync(Guid id);
    }
}