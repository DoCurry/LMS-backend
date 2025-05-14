using AutoMapper;
using Microsoft.EntityFrameworkCore;
using LMS_backend.Data;
using LMS_backend.Dtos;
using LMS_backend.Entities;
using LMS_backend.Services.Interfaces;

namespace LMS_backend.Services
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public AnnouncementService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<AnnouncementDto>> GetAllAnnouncementsAsync()
        {
            var announcements = await _context.Announcements
                .OrderByDescending(a => a.StartDate)
                .ToListAsync();

            return announcements.Select(a => _mapper.Map<AnnouncementDto>(a)!)
                .Where(a => a != null)
                .ToList();
        }

        public async Task<IEnumerable<AnnouncementDto>> GetActiveAnnouncementsAsync()
        {
            var now = DateTime.UtcNow;
            var announcements = await _context.Announcements
                .Where(a => a.IsActive &&
                           a.StartDate <= now &&
                           a.EndDate >= now)
                .OrderByDescending(a => a.StartDate)
                .ToListAsync();

            return announcements.Select(a => _mapper.Map<AnnouncementDto>(a)!)
                .Where(a => a != null)
                .ToList();
        }

        public async Task<AnnouncementDto?> GetAnnouncementByIdAsync(Guid id)
        {
            var announcement = await _context.Announcements
                .FirstOrDefaultAsync(a => a.Id == id);

            return announcement == null ? null : _mapper.Map<AnnouncementDto>(announcement);
        }

        public async Task<AnnouncementDto> CreateAnnouncementAsync(CreateAnnouncementDto createAnnouncementDto)
        {
            if (createAnnouncementDto.StartDate >= createAnnouncementDto.EndDate)
                throw new Exception("End date must be after start date");

            if (createAnnouncementDto.StartDate < DateTime.UtcNow)
                throw new Exception("Start date cannot be in the past");

            var announcement = _mapper.Map<Announcement>(createAnnouncementDto);
            if (announcement == null)
                throw new Exception("Failed to map announcement data");

            announcement.IsActive = true;
            announcement.CreatedAt = DateTime.UtcNow;

            _context.Announcements.Add(announcement);
            await _context.SaveChangesAsync();

            var announcementDto = _mapper.Map<AnnouncementDto>(announcement);
            if (announcementDto == null)
                throw new Exception("Failed to map created announcement data");

            return announcementDto;
        }

        public async Task<AnnouncementDto?> UpdateAnnouncementAsync(Guid id, UpdateAnnouncementDto updateAnnouncementDto)
        {
            var announcement = await _context.Announcements
                .FirstOrDefaultAsync(a => a.Id == id);

            if (announcement == null)
                return null;

            if (updateAnnouncementDto.StartDate.HasValue && 
                updateAnnouncementDto.EndDate.HasValue &&
                updateAnnouncementDto.StartDate >= updateAnnouncementDto.EndDate)
            {
                throw new Exception("End date must be after start date");
            }

            if (updateAnnouncementDto.StartDate.HasValue && 
                updateAnnouncementDto.StartDate < DateTime.UtcNow)
            {
                throw new Exception("Start date cannot be in the past");
            }

            _mapper.Map(updateAnnouncementDto, announcement);
            announcement.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var announcementDto = _mapper.Map<AnnouncementDto>(announcement);
            if (announcementDto == null)
                throw new Exception("Failed to map updated announcement data");

            return announcementDto;
        }

        public async Task<bool> DeleteAnnouncementAsync(Guid id)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null)
                return false;

            _context.Announcements.Remove(announcement);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleAnnouncementStatusAsync(Guid id)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null)
                return false;

            announcement.IsActive = !announcement.IsActive;
            announcement.LastUpdated = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}