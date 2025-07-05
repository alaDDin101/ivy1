using Application.Commons;
using Ivy.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class LookupsController : ControllerBase
{
    private readonly IvyContext _context;

    public LookupsController(IvyContext context)
    {
        _context = context;
    }

    [HttpGet("specialties")]
    public async Task<IActionResult> GetSpecialties([FromQuery] string? name)
    {
        var query = _context.Specialties.AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(s => s.Name.Contains(name));
        }

        var result = await query
            .Select(s => new { s.Id, s.Name })
            .ToListAsync();

        return Ok(new ApiResponse<Object>(200, "", result));
    }

    [HttpGet("cities")]
    public async Task<IActionResult> GetCities([FromQuery] string? name, [FromQuery] int? governorateId)
    {
        var query = _context.Cities.AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(c => c.Name.Contains(name));
        }

        if (governorateId.HasValue)
        {
            query = query.Where(c => c.Governorate == governorateId.Value);
        }

        var result = await query
            .Select(c => new { c.Id, c.Name })
            .ToListAsync();

        return Ok(new ApiResponse<Object>(200, "", result));
    }

    [HttpGet("clinics")]
    public async Task<IActionResult> GetClinics([FromQuery] string? name)
    {
        var query = _context.Clinics.AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(c => c.Name.Contains(name));
        }

        var result = await query
            .Select(c => new { c.Id, c.Name })
            .ToListAsync();

        return Ok(new ApiResponse<Object>(200, "", result));
    }
}
