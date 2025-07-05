namespace Application.Dto;

public class CityListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string GovernorateName { get; set; } = null!;
}

public class CreateCityDto
{
    public string Name { get; set; } = null!;
    public int GovernorateId { get; set; }
}

public class UpdateCityDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int GovernorateId { get; set; }
}
