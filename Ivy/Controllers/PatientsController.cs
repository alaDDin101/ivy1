
using Microsoft.AspNetCore.Mvc;
using Application.Commons;
using Application.Dto;
using Application.Interfaces.IRepositories.Patient;
using Application.Exceptions;
using System.Text.RegularExpressions;
using System.Security.Claims;

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
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized(new ApiResponse<string>(401, "Invalid user context."));

        var result = await _repository.GetPagedAsync(page, size, userId, roles);
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
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePatient(long id, [FromBody] PatientDto dto)
    {
        if (id != dto.Id)
            return BadRequest(new ApiResponse<string>(400, "Patient ID mismatch."));

        if (!Regex.IsMatch(dto.NationalNumber, @"^\d{11}$"))
        {
            return BadRequest(new ApiResponse<string>(400, "National number must be exactly 11 digits."));
        }
        try
        {
            var updatedPatient = await _repository.UpdatePatientAsync(dto);
            return Ok(new ApiResponse<PatientDto>(200, "Patient updated successfully.", updatedPatient));
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ApiResponse<string>(404, ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<string>(500, "An unexpected error occurred.", ex.Message));
        }
    }

}
