using Application.Commons;
using Application.Dto;
using Domain.Entities;

namespace Application.Interfaces.IRepositories.Patient
{
    public interface IPatientRepository
    {
        Task<PagedResult<PatientDto>> GetPagedAsync(int pageNumber, int pageSize, string userId, List<string> roles);
        Task<PatientDto> CreatePatientAsync(PatientDto patient);
        Task<PatientDto> UpdatePatientAsync(PatientDto dto);

    }
}
