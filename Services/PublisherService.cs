using AutoMapper;
using Microsoft.EntityFrameworkCore;
using LMS_backend.Data;
using LMS_backend.Dtos;
using LMS_backend.Entities;
using LMS_backend.Services.Interfaces;

namespace LMS_backend.Services
{
    public class PublisherService : IPublisherService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public PublisherService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PublisherDto>> GetAllPublishersAsync()
        {
            var publishers = await _context.Publishers
                .Include(p => p.Books)
                .ToListAsync();

            return publishers.Select(p => _mapper.Map<PublisherDto>(p)!)
                .Where(p => p != null)
                .ToList();
        }

        public async Task<PublisherDto?> GetPublisherByIdAsync(Guid id)
        {
            var publisher = await _context.Publishers
                .Include(p => p.Books)
                .FirstOrDefaultAsync(p => p.Id == id);

            return publisher == null ? null : _mapper.Map<PublisherDto>(publisher);
        }

        public async Task<PublisherDto> CreatePublisherAsync(CreatePublisherDto createPublisherDto)
        {
            if (await _context.Publishers.AnyAsync(p => p.Name == createPublisherDto.Name))
                throw new Exception("Publisher with this name already exists");

            var publisher = _mapper.Map<Publisher>(createPublisherDto);
            if (publisher == null)
                throw new Exception("Failed to map publisher data");

            _context.Publishers.Add(publisher);
            await _context.SaveChangesAsync();

            var publisherDto = _mapper.Map<PublisherDto>(publisher);
            if (publisherDto == null)
                throw new Exception("Failed to map created publisher data");

            return publisherDto;
        }

        public async Task<PublisherDto?> UpdatePublisherAsync(Guid id, UpdatePublisherDto updatePublisherDto)
        {
            var publisher = await _context.Publishers
                .Include(p => p.Books)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (publisher == null)
                return null;

            if (updatePublisherDto.Name != null && 
                await _context.Publishers.AnyAsync(p => p.Name == updatePublisherDto.Name && p.Id != id))
                throw new Exception("Publisher with this name already exists");

            _mapper.Map(updatePublisherDto, publisher);
            publisher.LastUpdated = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return _mapper.Map<PublisherDto>(publisher);
        }

        public async Task<bool> DeletePublisherAsync(Guid id)
        {
            var publisher = await _context.Publishers
                .Include(p => p.Books)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (publisher == null)
                return false;

            if (publisher.Books.Any())
                throw new Exception("Cannot delete publisher with associated books");

            _context.Publishers.Remove(publisher);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<BookDto>> GetPublisherBooksAsync(Guid id)
        {
            var publisher = await _context.Publishers
                .Include(p => p.Books)
                .ThenInclude(b => b.Authors)
                .Include(p => p.Books)
                .ThenInclude(b => b.Publishers)
                .Include(p => p.Books)
                .ThenInclude(b => b.Reviews)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (publisher == null)
                throw new Exception("Publisher not found");

            return publisher.Books
                .Select(b => _mapper.Map<BookDto>(b)!)
                .Where(b => b != null)
                .ToList();
        }
    }
}