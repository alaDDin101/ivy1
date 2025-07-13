namespace Application.Dto
{
    public class PatientLookupDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
    }

    public class DoctorLookupDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public string Specialty { get; set; } = null!;
        public string Email { get; set; } = null!;
    }

    public class DoctorClinicLookupDto
    {
        public long DoctorId { get; set; }
        public string DoctorName { get; set; } = null!;
        public long ClinicId { get; set; }
        public string ClinicName { get; set; } = null!;
    }

    public class AppointmentStatusLookupDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }
} 