using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dto
{
    public class AppointmentListDto
    {
        public long Id { get; set; }
        public string PatientName { get; set; } = null!;
        public DateTime Date { get; set; }
        public string Reason { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string ClinicName { get; set; } = null!;
        public string DoctorName { get; set; } = null!;
    }
    public class CreateAppointmentDto
    {
        public long PatientId { get; set; }
        public DateTime Date { get; set; }
        public string Reason { get; set; } = null!;
        public int StatusId { get; set; }
        public long DoctorClinicId { get; set; }
    }
    public class CreateAppointmentByPatientDto
    {
        public long PatientId { get; set; }
        public string Reason { get; set; } = null!;
        public long DoctorClinicId { get; set; }
    }
    public class UpdateAppointmentDto
    {
        public long Id { get; set; }
        public DateTime Date { get; set; }
        public string Reason { get; set; } = null!;
        public int StatusId { get; set; }
    }
    public class AcceptAppointmentByStaffDto
    {
        public long AppointmentId { get; set; }
        public DateTime ProposedDate { get; set; }
    }

    public class ConfirmAppointmentByPatientDto
    {
        public long AppointmentId { get; set; }
        public bool IsAccepted { get; set; }
    }
}
