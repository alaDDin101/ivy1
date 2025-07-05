namespace Application.Dto;

public class CreateClinicEmployeeDto
{
    public string FirstName { get; set; } = null!;
    public string FatherName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateOnly? BirthDate { get; set; }
    public string? Address { get; set; }
    public string NationalNumber { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Password { get; set; } = null!;
    public long ClinicId { get; set; }
    public string Role { get; set; } = "clinic-staff";
}

public class ClinicEmployeeDto : CreateClinicEmployeeDto
{
    public long Id { get; set; }
    public DateOnly From { get; set; }
    public DateOnly? To { get; set; }
}

public class UpdateClinicEmployeeInfoDto
{
    public long Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string FatherName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateOnly? BirthDate { get; set; }
    public string? Address { get; set; }
    public string NationalNumber { get; set; } = null!;
}

public class UpdateClinicEmployeeEmailDto
{
    public long Id { get; set; }
    public string Email { get; set; } = null!;
}

public class UpdateClinicEmployeePhoneDto
{
    public long Id { get; set; }
    public string PhoneNumber { get; set; } = null!;
}
