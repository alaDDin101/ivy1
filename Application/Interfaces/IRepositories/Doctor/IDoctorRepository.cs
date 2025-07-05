using Application.Commons;
using Application.Dto;

namespace Application.Interfaces.IRepositories.Doctor
{
    public interface IDoctorRepository
    {
        Task<PagedResult<DoctorDto>> GetPagedAsync(int page, int size);
        Task<DoctorDto> CreateAsync(CreateDoctorDto dto);
        Task<DoctorDto> UpdateAsync(DoctorDto dto);
        Task EditDoctorEmailAsync(EditDoctorEmailDto dto);
        Task EditDoctorPhoneAsync(EditDoctorPhoneDto dto);
        Task EditDoctorClinicAsync(EditDoctorClinicDto dto);
        Task EditDoctorInfoAsync(EditDoctorInfoDto dto);
    }
}
