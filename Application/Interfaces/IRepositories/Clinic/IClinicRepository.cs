using Application.Commons;
using Application.Dto;

namespace Application.Interfaces.IRepositories.Clinic
{
    public interface IClinicRepository
    {
        Task<PagedResult<ClinicListDto>> GetPagedAsync(int pageNumber, int pageSize);
        Task<ClinicListDto> CreateClinicAsync(CreateClinicDto dto);
        Task<ClinicListDto> UpdateClinicAsync(UpdateClinicDto dto);
    }
}
