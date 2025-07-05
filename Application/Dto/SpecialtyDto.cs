public class SpecialtyListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? EnName { get; set; }
}

public class CreateSpecialtyDto
{
    public string Name { get; set; } = null!;
    public string? EnName { get; set; }
}

public class UpdateSpecialtyDto
{
    public string Name { get; set; } = null!;
    public string? EnName { get; set; }
}
