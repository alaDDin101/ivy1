
using Microsoft.AspNetCore.Mvc;
using Application.Commons;
using Application.Dto;
using Application.Interfaces.IRepositories.Patient;
using Application.Exceptions;
using System.Text.RegularExpressions;
using System.Security.Claims;
using Application.Authorization;

/// <summary>
/// Patient management controller for handling patient operations
/// </summary>
/// <remarks>
/// This controller manages patient-related operations including:
/// - Retrieving paginated patient lists
/// - Individual patient lookup
/// - Patient information management
/// - Permission-based access control for different user types
/// </remarks>
[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly IPatientRepository _repository;

    /// <summary>
    /// Initializes a new instance of the PatientsController
    /// </summary>
    /// <param name="repository">Patient repository for data operations</param>
    public PatientsController(IPatientRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Get paginated list of patients
    /// </summary>
    /// <remarks>
    /// Retrieves a paginated list of patients with their personal information.
    /// This endpoint is typically used by clinic staff and administrators to view patient records.
    /// 
    /// Required Permission: ViewPatients
    /// </remarks>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="size">Page size (default: 10)</param>
    /// <returns>Paginated list of patients with their details</returns>
    /// <response code="200">Patients retrieved successfully</response>
    /// <response code="401">Unauthorized - Missing or invalid token</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("paged")]
    [Permission(Permissions.ViewPatients)]
    [ProducesResponseType(typeof(PagedResult<PatientDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<string>), 401)]
    [ProducesResponseType(typeof(ApiResponse<string>), 403)]
    [ProducesResponseType(typeof(ApiResponse<string>), 500)]
    public async Task<ActionResult<PagedResult<PatientDto>>> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new ApiResponse<string>(401, "Invalid user context."));

            var result = await _repository.GetPagedAsync(page, size, userId, roles);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<string>(500, "An error occurred while retrieving patients.", ex.Message));
        }
    }

    ///// <summary>
    ///// Get patient by ID
    ///// </summary>
    ///// <remarks>
    ///// Retrieves detailed information for a specific patient by their ID.
    ///// This includes personal information, contact details, and patient code.
    ///// 
    ///// Required Permission: ViewPatients
    ///// </remarks>
    ///// <param name="id">Patient ID</param>
    ///// <returns>Patient details</returns>
    ///// <response code="200">Patient retrieved successfully</response>
    ///// <response code="401">Unauthorized - Missing or invalid token</response>
    ///// <response code="403">Forbidden - Insufficient permissions</response>
    ///// <response code="404">Patient not found</response>
    ///// <response code="500">Internal server error</response>
    //[HttpGet("{id}")]
    //[Permission(Permissions.ViewPatients)]
    //[ProducesResponseType(typeof(ApiResponse<PatientDto>), 200)]
    //[ProducesResponseType(typeof(ApiResponse<string>), 401)]
    //[ProducesResponseType(typeof(ApiResponse<string>), 403)]
    //[ProducesResponseType(typeof(ApiResponse<string>), 404)]
    //[ProducesResponseType(typeof(ApiResponse<string>), 500)]
    //public async Task<ActionResult<ApiResponse<PatientDto>>> GetById(long id)
    //{
    //    try
    //    {
    //        var result = await _repository.GetByIdAsync(id);
    //        if (result == null)
    //        {
    //            return NotFound(new ApiResponse<string>(404, "Patient not found."));
    //        }
    //        return Ok(new ApiResponse<PatientDto>(200, "Patient retrieved successfully.", result));
    //    }
    //    catch (Exception ex)
    //    {
    //        return StatusCode(500, new ApiResponse<string>(500, "An error occurred while retrieving the patient.", ex.Message));
    //    }
    //}

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
