using Microsoft.AspNetCore.Mvc;
using Application.Commons;
using Application.Interfaces.IRepositories.ClinicEmployee;
using Application.Dto;
using Application.Authorization;

/// <summary>
/// Clinic employee management controller for handling clinic staff operations
/// </summary>
/// <remarks>
/// This controller manages clinic employee-related operations including:
/// - Retrieving paginated clinic employee lists
/// - Individual clinic employee lookup
/// - Clinic employee assignment and management
/// - Staff scheduling and clinic associations
/// - Permission-based access control for different user types
/// </remarks>
[ApiController]
[Route("api/[controller]")]
public class ClinicEmployeesController : ControllerBase
{
    private readonly IClinicEmployeeRepository _repository;

    /// <summary>
    /// Initializes a new instance of the ClinicEmployeesController
    /// </summary>
    /// <param name="repository">Clinic employee repository for data operations</param>
    public ClinicEmployeesController(IClinicEmployeeRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Get paginated list of clinic employees
    /// </summary>
    /// <remarks>
    /// Retrieves a paginated list of clinic employees with their personal information and clinic assignments.
    /// This endpoint is typically used by administrators to view and manage clinic staff.
    /// 
    /// Required Permission: ViewClinics (since this is clinic-related data)
    /// </remarks>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="size">Page size (default: 10)</param>
    /// <returns>Paginated list of clinic employees with their details</returns>
    /// <response code="200">Clinic employees retrieved successfully</response>
    /// <response code="401">Unauthorized - Missing or invalid token</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("paged")]
    [Permission(Permissions.ViewClinics)]
    [ProducesResponseType(typeof(PagedResult<ClinicEmployeeDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<string>), 401)]
    [ProducesResponseType(typeof(ApiResponse<string>), 403)]
    [ProducesResponseType(typeof(ApiResponse<string>), 500)]
    public async Task<ActionResult<PagedResult<ClinicEmployeeDto>>> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        try
        {
            var result = await _repository.GetPagedAsync(page, size);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<string>(500, "An error occurred while retrieving clinic employees.", ex.Message));
        }
    }

    /// <summary>
    /// Get clinic employee by ID
    /// </summary>
    /// <remarks>
    /// Retrieves detailed information for a specific clinic employee by their ID.
    /// This includes personal information, clinic assignment, and employment dates.
    /// 
    /// Required Permission: ViewClinics
    /// </remarks>
    /// <param name="id">Clinic employee ID</param>
    /// <returns>Clinic employee details</returns>
    /// <response code="200">Clinic employee retrieved successfully</response>
    /// <response code="401">Unauthorized - Missing or invalid token</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    /// <response code="404">Clinic employee not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id}")]
    [Permission(Permissions.ViewClinics)]
    // Note: GetById method is commented out because IClinicEmployeeRepository doesn't have a GetByIdAsync method
    /*
    [ProducesResponseType(typeof(ApiResponse<ClinicEmployeeDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<string>), 401)]
    [ProducesResponseType(typeof(ApiResponse<string>), 403)]
    [ProducesResponseType(typeof(ApiResponse<string>), 404)]
    [ProducesResponseType(typeof(ApiResponse<string>), 500)]
    public async Task<ActionResult<ApiResponse<ClinicEmployeeDto>>> GetById(long id)
    {
        try
        {
            var result = await _repository.GetByIdAsync(id);
            if (result == null)
            {
                return NotFound(new ApiResponse<string>(404, "Clinic employee not found."));
            }
            return Ok(new ApiResponse<ClinicEmployeeDto>(200, "Clinic employee retrieved successfully.", result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<string>(500, "An error occurred while retrieving the clinic employee.", ex.Message));
        }
    }
    */

    /// <summary>
    /// Create a new clinic employee assignment
    /// </summary>
    /// <remarks>
    /// Creates a new clinic employee assignment linking a person to a clinic.
    /// This endpoint is typically used by administrators to assign staff to clinics.
    /// 
    /// Required Permission: CreateClinics (since this modifies clinic staff)
    /// </remarks>
    /// <param name="dto">Clinic employee creation details</param>
    /// <returns>Created clinic employee assignment details</returns>
    /// <response code="201">Clinic employee created successfully</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Unauthorized - Missing or invalid token</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [Permission(Permissions.CreateClinics)]
    [ProducesResponseType(typeof(ApiResponse<ClinicEmployeeDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse<string>), 400)]
    [ProducesResponseType(typeof(ApiResponse<string>), 401)]
    [ProducesResponseType(typeof(ApiResponse<string>), 403)]
    [ProducesResponseType(typeof(ApiResponse<string>), 500)]
    public async Task<IActionResult> Create([FromBody] CreateClinicEmployeeDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<string>(400, "Invalid input data."));
            }

            var result = await _repository.CreateAsync(dto);
            return Ok(new ApiResponse<ClinicEmployeeDto>(201, "Clinic employee created successfully.", result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<string>(500, "An error occurred while creating the clinic employee.", ex.Message));
        }
    }

    /// <summary>
    /// Update clinic employee assignment
    /// </summary>
    /// <remarks>
    /// Updates clinic employee assignment information such as employment dates or clinic assignment.
    /// This endpoint is typically used by administrators to modify staff assignments.
    /// 
    /// Required Permission: UpdateClinics
    /// </remarks>
    /// <param name="dto">Updated clinic employee details</param>
    /// <returns>Updated clinic employee assignment details</returns>
    /// <response code="200">Clinic employee updated successfully</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Unauthorized - Missing or invalid token</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    /// <response code="404">Clinic employee not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut]
    [Permission(Permissions.UpdateClinics)]
    [ProducesResponseType(typeof(ApiResponse<ClinicEmployeeDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<string>), 400)]
    [ProducesResponseType(typeof(ApiResponse<string>), 401)]
    [ProducesResponseType(typeof(ApiResponse<string>), 403)]
    [ProducesResponseType(typeof(ApiResponse<string>), 404)]
    [ProducesResponseType(typeof(ApiResponse<string>), 500)]
    public async Task<IActionResult> Update([FromBody] UpdateClinicEmployeeInfoDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<string>(400, "Invalid input data."));
            }

            var result = await _repository.UpdateInfoAsync(dto);
            return Ok(new ApiResponse<ClinicEmployeeDto>(200, "Clinic employee updated successfully.", result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<string>(500, "An error occurred while updating the clinic employee.", ex.Message));
        }
    }

    /// <summary>
    /// Get employees for a specific clinic
    /// </summary>
    /// <remarks>
    /// Retrieves all employees assigned to a specific clinic.
    /// This endpoint is useful for clinic management and staff scheduling.
    /// 
    /// Required Permission: ViewClinics
    /// </remarks>
    /// <param name="clinicId">Clinic ID</param>
    /// <returns>List of employees for the specified clinic</returns>
    /// <response code="200">Clinic employees retrieved successfully</response>
    /// <response code="401">Unauthorized - Missing or invalid token</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    /// <response code="500">Internal server error</response>
    // Note: GetByClinic method is commented out because IClinicEmployeeRepository doesn't have a GetEmployeesByClinicAsync method
    /*
    [HttpGet("clinic/{clinicId}")]
    [Permission(Permissions.ViewClinics)]
    [ProducesResponseType(typeof(ApiResponse<List<ClinicEmployeeDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<string>), 401)]
    [ProducesResponseType(typeof(ApiResponse<string>), 403)]
    [ProducesResponseType(typeof(ApiResponse<string>), 500)]
    public async Task<ActionResult<ApiResponse<List<ClinicEmployeeDto>>>> GetByClinic(long clinicId)
    {
        try
        {
            var result = await _repository.GetEmployeesByClinicAsync(clinicId);
            return Ok(new ApiResponse<List<ClinicEmployeeDto>>(200, "Clinic employees retrieved successfully.", result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<string>(500, "An error occurred while retrieving clinic employees.", ex.Message));
        }
    }
    */
}
