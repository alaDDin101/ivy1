using Application.Commons;
using Application.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.IRepositories.ClinicEmployee
{
    public interface IClinicEmployeeRepository
    {
        Task<PagedResult<ClinicEmployeeDto>> GetPagedAsync(int page, int size);
        Task<ClinicEmployeeDto> CreateAsync(CreateClinicEmployeeDto dto);
        Task<ClinicEmployeeDto> UpdateInfoAsync(UpdateClinicEmployeeInfoDto dto);
        Task UpdateEmailAsync(UpdateClinicEmployeeEmailDto dto);
        Task UpdatePhoneAsync(UpdateClinicEmployeePhoneDto dto);
        Task RemoveFromClinicAsync(long personId);
    }

}
