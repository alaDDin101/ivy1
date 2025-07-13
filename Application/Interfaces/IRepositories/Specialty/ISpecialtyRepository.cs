using Application.Dto;

namespace Application.Interfaces.IRepositories.Specialty
{
    public interface ISpecialtyRepository
    {
        Task<List<SpecialtyListDto>> GetAllAsync();
        Task<SpecialtyListDto> CreateAsync(CreateSpecialtyDto dto);
        Task<SpecialtyListDto> UpdateAsync(int id, UpdateSpecialtyDto dto);
    }
}
