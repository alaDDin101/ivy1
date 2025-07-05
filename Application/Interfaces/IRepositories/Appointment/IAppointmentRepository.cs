using Application.Commons;
using Application.Dto;

namespace Application.Interfaces.IRepositories.Appointment
{
    public interface IAppointmentRepository
    {
        Task<AppointmentListDto> BookAppointmentAsync(CreateAppointmentDto dto);
        Task<PagedResult<AppointmentListDto>> GetPagedAppointmentsAsync(int pageNumber, int pageSize);
        Task<AppointmentListDto> UpdateAppointmentAsync(UpdateAppointmentDto dto); // optional
    }
}
