using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class SpecialtiesController : ControllerBase
{
    private readonly ISpecialtyRepository _repo;

    public SpecialtiesController(ISpecialtyRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<ActionResult<List<SpecialtyListDto>>> GetAll()
    {
        return Ok(await _repo.GetAllAsync());
    }

    [HttpPost]
    public async Task<ActionResult<SpecialtyListDto>> Create(CreateSpecialtyDto dto)
    {
        return Ok(await _repo.CreateAsync(dto));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<SpecialtyListDto>> Update(int id, UpdateSpecialtyDto dto)
    {
        return Ok(await _repo.UpdateAsync(id, dto));
    }
}
