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
                DoctorClinic = dto.DoctorClinicId,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            _context.Appointments.Add(entity);
            await _context.SaveChangesAsync();

            return await GetAppointmentDetailsAsync(entity.Id);
        }

        public async Task<AppointmentListDto> BookAppointmentByPatientAsync(CreateAppointmentByPatientDto dto)
        {
            var entity = new Domain.Entities.Appointment
            {
                Patient = dto.PatientId,
                Date = null, // No date set, pending
                Reason = dto.Reason,
                Status = 1, // Pending
                DoctorClinic = dto.DoctorClinicId,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            _context.Appointments.Add(entity);
            await _context.SaveChangesAsync();

            return await GetAppointmentDetailsAsync(entity.Id);
        }

        public async Task<AppointmentListDto> AcceptAppointmentByStaffAsync(long appointmentId, DateTime proposedDate)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null)
                throw new Exception("Appointment not found");

            appointment.Date = proposedDate;
            appointment.Status = 5; // WaitingForPatientConfirmation
            appointment.LastUpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetAppointmentDetailsAsync(appointmentId);
        }

        public async Task<AppointmentListDto> ConfirmAppointmentByPatientAsync(long appointmentId, bool isAccepted)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null)
                throw new Exception("Appointment not found");

            if (isAccepted)
            {
                appointment.Status = 2; // Scheduled
            }
            else
            {
                appointment.Status = 1; // Pending
                appointment.Date = null; // Remove the proposed date
            }

            appointment.LastUpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return await GetAppointmentDetailsAsync(appointmentId);
        }

        private async Task<AppointmentListDto> GetAppointmentDetailsAsync(long appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.PatientNavigation).ThenInclude(p => p.PersonNavigation)
                .Include(a => a.DoctorClinicNavigation).ThenInclude(dc => dc.ClinicNavigation)
                .Include(a => a.DoctorClinicNavigation).ThenInclude(dc => dc.DoctorNavigation).ThenInclude(d => d.PersonNavigation)
                .Include(a => a.StatusNavigation)
                .FirstAsync(a => a.Id == appointmentId);

            return new AppointmentListDto
            {
                Id = appointment.Id,
                PatientName = $"{appointment.PatientNavigation.PersonNavigation.FirstName} {appointment.PatientNavigation.PersonNavigation.LastName}",
                Date = appointment.Date ?? DateTime.MinValue, // Handle nullable
                Reason = appointment.Reason,
                Status = appointment.StatusNavigation.Name,
                ClinicName = appointment.DoctorClinicNavigation.ClinicNavigation.Name,
                DoctorName = $"{appointment.DoctorClinicNavigation.DoctorNavigation.PersonNavigation.FirstName} {appointment.DoctorClinicNavigation.DoctorNavigation.PersonNavigation.LastName}"
            };
        }

        public async Task<PagedResult<AppointmentListDto>> GetPagedAppointmentsAsync(int pageNumber, int pageSize, string userId, List<string> roles)
        {
            var query = _context.Appointments
                .Include(a => a.PatientNavigation).ThenInclude(p => p.PersonNavigation)
                .Include(a => a.DoctorClinicNavigation).ThenInclude(dc => dc.ClinicNavigation)
                .Include(a => a.DoctorClinicNavigation).ThenInclude(dc => dc.DoctorNavigation).ThenInclude(d => d.PersonNavigation)
                .Include(a => a.StatusNavigation)
                .AsNoTracking();

            // Apply role-based filtering
            if (roles.Contains("patient"))
            {
                // Patients see only their own appointments
                var patientId = await _context.Patients
                    .Where(p => p.PersonNavigation.PartyNavigation.User.MembershipUser == userId)
                    .Select(p => p.Person)
                    .FirstOrDefaultAsync();
                
                query = query.Where(a => a.Patient == patientId);
            }
            else if (roles.Contains("doctor"))
            {
                // Doctors see appointments for their doctor clinics
                var doctorId = await _context.Doctors
                    .Where(d => d.PersonNavigation.PartyNavigation.User.MembershipUser == userId)
                    .Select(d => d.Person)
                    .FirstOrDefaultAsync();
                
                var doctorClinicIds = await _context.DoctorClinics
                    .Where(dc => dc.Doctor == doctorId)
                    .Select(dc => dc.Id)
                    .ToListAsync();
                
                query = query.Where(a => doctorClinicIds.Contains(a.DoctorClinic));
            }
            else if (roles.Contains("clinic-staff"))
            {
                // Clinic staff see appointments for their clinic
                var clinicId = await _context.ClinicEmplyees
                    .Where(ce => ce.PersonNavigation.PartyNavigation.User.MembershipUser == userId)
                    .Select(ce => ce.Clinic)
                    .FirstOrDefaultAsync();
                
                var clinicDoctorIds = await _context.DoctorClinics
                    .Where(dc => dc.Clinic == clinicId)
                    .Select(dc => dc.Id)
                    .ToListAsync();
                
                query = query.Where(a => clinicDoctorIds.Contains(a.DoctorClinic));
            }
            // Admin role sees all appointments (no filtering)

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(a => a.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AppointmentListDto
                {
                    Id = a.Id,
                    PatientName = $"{a.PatientNavigation.PersonNavigation.FirstName} {a.PatientNavigation.PersonNavigation.LastName}",
                    Date = a.Date ?? DateTime.MinValue, // Handle nullable
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
            appointment.LastUpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetAppointmentDetailsAsync(dto.Id);
        }
    }
}
