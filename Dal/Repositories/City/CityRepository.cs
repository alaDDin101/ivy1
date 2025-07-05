using Application.Commons;
using Application.Dto;
using Application.Interfaces.IRepositories.City;
using Ivy.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class CityRepository : ICityRepository
    {

        private readonly IvyContext _context;

        public CityRepository(IvyContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<CityListDto>> GetPagedAsync(int pageNumber, int pageSize)
        {
            var query = _context.Cities.Include(c => c.GovernorateNavigation).AsNoTracking();

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(c => c.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CityListDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    GovernorateName = c.GovernorateNavigation.Name
                })
                .ToListAsync();

            return new PagedResult<CityListDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageSize = pageSize,
                PageNumber = pageNumber
            };
        }

        public async Task<CityListDto> CreateAsync(CreateCityDto dto)
        {
            var city = new Domain.Entities.City
            {
                Name = dto.Name,
                Governorate = dto.GovernorateId
            };

            _context.Cities.Add(city);
            await _context.SaveChangesAsync();

            var governorate = await _context.Governorates.FindAsync(dto.GovernorateId);

            return new CityListDto
            {
                Id = city.Id,
                Name = city.Name,
                GovernorateName = governorate!.Name
            };
        }

        public async Task<CityListDto> UpdateAsync(UpdateCityDto dto)
        {
            var city = await _context.Cities.FindAsync(dto.Id)
                       ?? throw new Exception("City not found");

            city.Name = dto.Name;
            city.Governorate = dto.GovernorateId;

            await _context.SaveChangesAsync();

            var governorate = await _context.Governorates.FindAsync(dto.GovernorateId);

            return new CityListDto
            {
                Id = city.Id,
                Name = city.Name,
                GovernorateName = governorate!.Name
            };
        }
    }
}
