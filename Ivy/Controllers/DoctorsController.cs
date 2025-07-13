using Microsoft.AspNetCore.Mvc;
using Application.Commons;
using Application.Interfaces.IRepositories.Doctor;
using Application.Dto;
using Application.Authorization;

/// <summary>
/// Doctor management controller for handling doctor operations
/// </summary>
/// <remarks>
/// This controller manages doctor-related operations including:
/// - Retrieving paginated doctor lists
/// - Individual doctor lookup
/// - Doctor profile management
/// - Doctor-clinic associations
/// - Permission-based access control for different user types
/// </remarks>
[ApiController]
[Route("api/[controller]")]
public class DoctorsController : ControllerBase
{
    private readonly IDoctorRepository _repository;

    /// <summary>
    /// Initializes a new instance of the DoctorsController
    /// </summary>
    /// <param name="repository">Doctor repository for data operations</param>
    public DoctorsController(IDoctorRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Get paginated list of doctors
    /// </summary>
    /// <remarks>
    /// Retrieves a paginated list of doctors with their personal information, specialties, and clinic associations.
    /// This endpoint is typically used by clinic staff and administrators to view doctor records.
    /// 
    /// Required Permission: ViewDoctors
    /// </remarks>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="size">Page size (default: 10)</param>
    /// <returns>Paginated list of doctors with their details</returns>
    /// <response code="200">Doctors retrieved successfully</response>
    /// <response code="401">Unauthorized - Missing or invalid token</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("paged")]
    [Permission(Permissions.ViewDoctors)]
    [ProducesResponseType(typeof(PagedResult<DoctorDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<string>), 401)]
    [ProducesResponseType(typeof(ApiResponse<string>), 403)]
    [ProducesResponseType(typeof(ApiResponse<string>), 500)]
    public async Task<ActionResult<PagedResult<DoctorDto>>> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        try
        {
            var result = await _repository.GetPagedAsync(page, size);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<string>(500, "An error occurred while retrieving doctors.", ex.Message));
        }
    }

    // Note: GetById method is commented out because IDoctorRepository doesn't have a GetByIdAsync method
    /*
    /// <summary>
    /// Get doctor by ID
    /// </summary>
    /// <remarks>
    /// Retrieves detailed information for a specific doctor by their ID.
    /// This includes personal information, specialty, description, and clinic associations.
    /// 
    /// Required Permission: ViewDoctors
    /// </remarks>
    /// <param name="id">Doctor ID</param>
    /// <returns>Doctor details</returns>
    /// <response code="200">Doctor retrieved successfully</response>
    /// <response code="401">Unauthorized - Missing or invalid token</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    /// <response code="404">Doctor not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id}")]
    [Permission(Permissions.ViewDoctors)]
    [ProducesResponseType(typeof(ApiResponse<DoctorDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<string>), 401)]
    [ProducesResponseType(typeof(ApiResponse<string>), 403)]
    [ProducesResponseType(typeof(ApiResponse<string>), 404)]
    [ProducesResponseType(typeof(ApiResponse<string>), 500)]
    public async Task<ActionResult<ApiResponse<DoctorDto>>> GetById(long id)
    {
        try
        {
            var result = await _repository.GetByIdAsync(id);
            if (result == null)
            {
                return NotFound(new ApiResponse<string>(404, "Doctor not found."));
            }
            return Ok(new ApiResponse<DoctorDto>(200, "Doctor retrieved successfully.", result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<string>(500, "An error occurred while retrieving the doctor.", ex.Message));
        }
    }
    */

    /// <summary>
    /// Create a new doctor
    /// </summary>
    /// <remarks>
    /// Creates a new doctor profile with personal information, specialty, and clinic associations.
    /// This endpoint is typically used by administrators to add new doctors to the system.
    /// 
    /// Required Permission: CreateDoctors
    /// </remarks>
    /// <param name="dto">Doctor creation details</param>
    /// <returns>Created doctor details</returns>
    /// <response code="201">Doctor created successfully</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Unauthorized - Missing or invalid token</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [Permission(Permissions.CreateDoctors)]
    [ProducesResponseType(typeof(ApiResponse<DoctorDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse<string>), 400)]
    [ProducesResponseType(typeof(ApiResponse<string>), 401)]
    [ProducesResponseType(typeof(ApiResponse<string>), 403)]
    [ProducesResponseType(typeof(ApiResponse<string>), 500)]
    public async Task<IActionResult> Create([FromBody] CreateDoctorDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<string>(400, "Invalid input data."));
            }

            var result = await _repository.CreateAsync(dto);
            return Ok(new ApiResponse<DoctorDto>(201, "Doctor created successfully.", result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<string>(500, "An error occurred while creating the doctor.", ex.Message));
        }
    }

    /// <summary>
    /// Update doctor information
    /// </summary>
    /// <remarks>
    /// Updates doctor profile information including personal details, specialty, and description.
    /// This endpoint is typically used by administrators to modify doctor information.
    /// 
    /// Required Permission: UpdateDoctors
    /// </remarks>
    /// <param name="dto">Updated doctor details</param>
    /// <returns>Updated doctor details</returns>
    /// <response code="200">Doctor updated successfully</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Unauthorized - Missing or invalid token</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    /// <response code="404">Doctor not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut]
    [Permission(Permissions.UpdateDoctors)]
    [ProducesResponseType(typeof(ApiResponse<DoctorDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<string>), 400)]
    [ProducesResponseType(typeof(ApiResponse<string>), 401)]
    [ProducesResponseType(typeof(ApiResponse<string>), 403)]
    [ProducesResponseType(typeof(ApiResponse<string>), 404)]
    [ProducesResponseType(typeof(ApiResponse<string>), 500)]
    public async Task<IActionResult> Update([FromBody] DoctorDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<string>(400, "Invalid input data."));
            }

            var result = await _repository.UpdateAsync(dto);
            return Ok(new ApiResponse<DoctorDto>(200, "Doctor updated successfully.", result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<string>(500, "An error occurred while updating the doctor.", ex.Message));
        }
    }
}
