namespace Application.Dto;
public class ClinicListDto
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string CityName { get; set; } = null!;
    public List<string> Doctors { get; set; } = new();
}

public class CreateClinicDto
{
    public string Name { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string DetailedAddress { get; set; } = null!;
    public int CityId { get; set; }
    
    public List<long> DoctorIds { get; set; } = new();
    public DateOnly From { get; set; } // Comes from frontend
}

public class UpdateClinicDto
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string DetailedAddress { get; set; } = null!;
    public int CityId { get; set; }
    public List<long> DoctorIds { get; set; } = new();
    public DateOnly From { get; set; } // Comes from frontend
}