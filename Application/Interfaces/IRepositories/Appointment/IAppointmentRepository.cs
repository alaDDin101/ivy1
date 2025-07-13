using Application.Commons;
using Application.Dto;

namespace Application.Interfaces.IRepositories.Appointment
{
    public interface IAppointmentRepository
    {
        Task<AppointmentListDto> BookAppointmentAsync(CreateAppointmentDto dto);
        Task<AppointmentListDto> BookAppointmentByPatientAsync(CreateAppointmentByPatientDto dto);
        Task<AppointmentListDto> AcceptAppointmentByStaffAsync(long appointmentId, DateTime proposedDate);
        Task<AppointmentListDto> ConfirmAppointmentByPatientAsync(long appointmentId, bool isAccepted);
        Task<PagedResult<AppointmentListDto>> GetPagedAppointmentsAsync(int pageNumber, int pageSize, string userId, List<string> roles);
        Task<AppointmentListDto> UpdateAppointmentAsync(UpdateAppointmentDto dto);
    }
}
