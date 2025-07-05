using System;

namespace Application.Dto
{
    public class DoctorDto
    {
        public long Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string FatherName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public DateOnly? BirthDate { get; set; }
        public string? Address { get; set; }
        public string NationalNumber { get; set; } = null!;
        public string? Image { get; set; }
        public string? Description { get; set; }
        public int SpecialtyId { get; set; }
        public string SpecialtyName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Phonenumber { get; set; } = null!;
    }
    public class CreateDoctorDto
    {
        public string FirstName { get; set; } = null!;
        public string FatherName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public DateOnly? BirthDate { get; set; }
        public string? Address { get; set; }
        public string NationalNumber { get; set; } = null!;
        public string? Image { get; set; }
        public string? Description { get; set; }
        public int SpecialtyId { get; set; }
        public long ClinicId { get; set; }
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
    }
    public class EditDoctorInfoDto
    {
        public long Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string FatherName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public DateOnly? BirthDate { get; set; }
        public string? Address { get; set; }
        public string NationalNumber { get; set; } = null!;
        public string? Image { get; set; }
        public string? Description { get; set; }
        public int SpecialtyId { get; set; }
    }
    public class EditDoctorClinicDto
    {
        public long DoctorId { get; set; }
        public int NewClinicId { get; set; }
    }
    public class EditDoctorPhoneDto
    {
        public long DoctorId { get; set; }
        public string NewPhoneNumber { get; set; } = null!;
    }
    public class EditDoctorEmailDto
    {
        public long DoctorId { get; set; }
        public string NewEmail { get; set; } = null!;
    }
}
