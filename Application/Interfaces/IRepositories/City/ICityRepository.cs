using Application.Commons;
using Application.Dto;

namespace Application.Interfaces.IRepositories.City
{
    public interface ICityRepository
    {
        Task<PagedResult<CityListDto>> GetPagedAsync(int pageNumber, int pageSize);
        Task<CityListDto> CreateAsync(CreateCityDto dto);
        Task<CityListDto> UpdateAsync(UpdateCityDto dto);
    }
}
