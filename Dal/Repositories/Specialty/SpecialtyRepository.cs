using Domain.Entities;
using Ivy.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Application.Interfaces.IRepositories.Specialty;
using Application.Dto;

namespace Infrastructure.Repositories
{
    public class SpecialtyRepository : ISpecialtyRepository
    {
        private readonly IvyContext _context;

        public SpecialtyRepository(IvyContext context)
        {
            _context = context;
        }

        public async Task<List<SpecialtyListDto>> GetAllAsync()
        {
            return await _context.Specialties
                .Select(s => new SpecialtyListDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    EnName = s.EnName
                }).ToListAsync();
        }

        public async Task<SpecialtyListDto> CreateAsync(CreateSpecialtyDto dto)
        {
            var entity = new Specialty
            {
                Name = dto.Name,
                EnName = dto.EnName
            };

            _context.Specialties.Add(entity);
            await _context.SaveChangesAsync();

            return new SpecialtyListDto
            {
                Id = entity.Id,
                Name = entity.Name,
                EnName = entity.EnName
            };
        }

        public async Task<SpecialtyListDto> UpdateAsync(int id, UpdateSpecialtyDto dto)
        {
            var entity = await _context.Specialties.FindAsync(id);
            if (entity == null)
                throw new KeyNotFoundException("Specialty not found.");

            entity.Name = dto.Name;
            entity.EnName = dto.EnName;

            await _context.SaveChangesAsync();

            return new SpecialtyListDto
            {
                Id = entity.Id,
                Name = entity.Name,
                EnName = entity.EnName
            };
        }
    }
}
