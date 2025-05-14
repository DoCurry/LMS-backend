using AutoMapper;
using Microsoft.EntityFrameworkCore;
using LMS_backend.Data;
using LMS_backend.Dtos;
using LMS_backend.Entities;
using LMS_backend.Services.Interfaces;

namespace LMS_backend.Services
{
    public class AuthorService : IAuthorService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public AuthorService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<AuthorDto>> GetAllAuthorsAsync()
        {
            var authors = await _context.Authors
                .Include(a => a.Books)
                .ToListAsync();

            return authors.Select(a => _mapper.Map<AuthorDto>(a)!)
                .Where(a => a != null)
                .ToList();
        }

        public async Task<AuthorDto?> GetAuthorByIdAsync(Guid id)
        {
            var author = await _context.Authors
                .Include(a => a.Books)
                .FirstOrDefaultAsync(a => a.Id == id);

            return author == null ? null : _mapper.Map<AuthorDto>(author);
        }

        public async Task<AuthorDto> CreateAuthorAsync(CreateAuthorDto createAuthorDto)
        {
            if (await _context.Authors.AnyAsync(a => a.Name == createAuthorDto.Name))
                throw new Exception("Author with this name already exists");

            var author = _mapper.Map<Author>(createAuthorDto);
            if (author == null)
                throw new Exception("Failed to map author data");

            _context.Authors.Add(author);
            await _context.SaveChangesAsync();

            var authorDto = _mapper.Map<AuthorDto>(author);
            if (authorDto == null)
                throw new Exception("Failed to map created author data");

            return authorDto;
        }

        public async Task<AuthorDto?> UpdateAuthorAsync(Guid id, UpdateAuthorDto updateAuthorDto)
        {
            var author = await _context.Authors
                .Include(a => a.Books)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (author == null)
                return null;

            if (updateAuthorDto.Name != null && 
                await _context.Authors.AnyAsync(a => a.Name == updateAuthorDto.Name && a.Id != id))
                throw new Exception("Author with this name already exists");

            _mapper.Map(updateAuthorDto, author);
            author.LastUpdated = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return _mapper.Map<AuthorDto>(author);
        }

        public async Task<bool> DeleteAuthorAsync(Guid id)
        {
            var author = await _context.Authors
                .Include(a => a.Books)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (author == null)
                return false;

            if (author.Books.Any())
                throw new Exception("Cannot delete author with associated books");

            _context.Authors.Remove(author);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<BookDto>> GetAuthorBooksAsync(Guid id)
        {
            var author = await _context.Authors
                .Include(a => a.Books)
                .ThenInclude(b => b.Authors)
                .Include(a => a.Books)
                .ThenInclude(b => b.Publishers)
                .Include(a => a.Books)
                .ThenInclude(b => b.Reviews)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (author == null)
                throw new Exception("Author not found");

            return author.Books
                .Select(b => _mapper.Map<BookDto>(b)!)
                .Where(b => b != null)
                .ToList();
        }
    }
}