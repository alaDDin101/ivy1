
using Microsoft.AspNetCore.Mvc;
using Application.Commons;
using Application.Dto;
using Application.Interfaces.IRepositories.Patient;
using Application.Exceptions;

[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly IPatientRepository _repository;

    public PatientsController(IPatientRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("paged")]
    public async Task<ActionResult<PagedResult<PatientDto>>> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        var result = await _repository.GetPagedAsync(page, size);
        return Ok(result);
    }
    [HttpPost]
    public async Task<IActionResult> CreatePatient([FromBody] PatientDto dto)
    {
        try
        {
            var result = await _repository.CreatePatientAsync(dto);
            return Ok(new ApiResponse<PatientDto>(200, "Patient created successfully.", result));
        }
        catch (PatientAlreadyExistsException ex)
        {
            return Conflict(new ApiResponse<string>(409, ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<string>(500, "An unexpected error occurred.", ex.Message));
        }
    }
}
