using Application.Commons;
using Application.Dto;
using Application.Interfaces.IRepositories.Appointment;
using Ivy.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Infrastructure.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly IvyContext _context;

        public AppointmentRepository(IvyContext context)
        {
            _context = context;
        }

        public async Task<AppointmentListDto> BookAppointmentAsync(CreateAppointmentDto dto)
        {
            var entity = new Domain.Entities.Appointment
            {
                Patient = dto.PatientId,
                Date = dto.Date,
                Reason = dto.Reason,
                Status = dto.StatusId,
                DoctorClinic = dto.DoctorClinicId
            };

            _context.Appointments.Add(entity);
            await _context.SaveChangesAsync();

            var patient = await _context.Patients.Include(p => p.PersonNavigation).FirstAsync(p => p.Person == dto.PatientId);
            var doctorClinic = await _context.DoctorClinics
                .Include(dc => dc.ClinicNavigation)
                .Include(dc => dc.DoctorNavigation).ThenInclude(d => d.PersonNavigation)
                .FirstAsync(dc => dc.Id == dto.DoctorClinicId);
            var status = await _context.AppointmentStatuses.FirstAsync(s => s.Id == dto.StatusId);

            return new AppointmentListDto
            {
                Id = entity.Id,
                PatientName = $"{patient.PersonNavigation.FirstName} {patient.PersonNavigation.LastName}",
                Date = entity.Date,
                Reason = entity.Reason,
                Status = status.Name,
                ClinicName = doctorClinic.ClinicNavigation.Name,
                DoctorName = $"{doctorClinic.DoctorNavigation.PersonNavigation.FirstName} {doctorClinic.DoctorNavigation.PersonNavigation.LastName}"
            };
        }

        public async Task<PagedResult<AppointmentListDto>> GetPagedAppointmentsAsync(int pageNumber, int pageSize)
        {
            var query = _context.Appointments
                .Include(a => a.PatientNavigation).ThenInclude(p => p.PersonNavigation)
                .Include(a => a.DoctorClinicNavigation).ThenInclude(dc => dc.ClinicNavigation)
                .Include(a => a.DoctorClinicNavigation).ThenInclude(dc => dc.DoctorNavigation).ThenInclude(d => d.PersonNavigation)
                .Include(a => a.StatusNavigation)
                .AsNoTracking();

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(a => a.Date)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AppointmentListDto
                {
                    Id = a.Id,
                    PatientName = $"{a.PatientNavigation.PersonNavigation.FirstName} {a.PatientNavigation.PersonNavigation.LastName}",
                    Date = a.Date,
                    Reason = a.Reason,
                    Status = a.StatusNavigation.Name,
                    ClinicName = a.DoctorClinicNavigation.ClinicNavigation.Name,
                    DoctorName = $"{a.DoctorClinicNavigation.DoctorNavigation.PersonNavigation.FirstName} {a.DoctorClinicNavigation.DoctorNavigation.PersonNavigation.LastName}"
                })
                .ToListAsync();

            return new PagedResult<AppointmentListDto>
            {
                Items = items,
                PageSize = pageSize,
                PageNumber = pageNumber,
                TotalCount = totalCount
            };
        }

        public async Task<AppointmentListDto> UpdateAppointmentAsync(UpdateAppointmentDto dto)
        {
            var appointment = await _context.Appointments.FindAsync(dto.Id);
            if (appointment == null)
                throw new Exception("Appointment not found");

            appointment.Date = dto.Date;
            appointment.Reason = dto.Reason;
            appointment.Status = dto.StatusId;

            await _context.SaveChangesAsync();

            // Map as ListDto (re-fetch with necessary joins if needed)
            var patient = await _context.Patients.Include(p => p.PersonNavigation).FirstAsync(p => p.Person == appointment.Patient);
            var doctorClinic = await _context.DoctorClinics
                .Include(dc => dc.ClinicNavigation)
                .Include(dc => dc.DoctorNavigation).ThenInclude(d => d.PersonNavigation)
                .FirstAsync(dc => dc.Id == appointment.DoctorClinic);
            var status = await _context.AppointmentStatuses.FirstAsync(s => s.Id == dto.StatusId);

            return new AppointmentListDto
            {
                Id = appointment.Id,
                PatientName = $"{patient.PersonNavigation.FirstName} {patient.PersonNavigation.LastName}",
                Date = appointment.Date,
                Reason = appointment.Reason,
                Status = status.Name,
                ClinicName = doctorClinic.ClinicNavigation.Name,
                DoctorName = $"{doctorClinic.DoctorNavigation.PersonNavigation.FirstName} {doctorClinic.DoctorNavigation.PersonNavigation.LastName}"
            };
        }
    }
}
