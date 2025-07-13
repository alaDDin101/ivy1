using Microsoft.AspNetCore.Mvc;
using Application.Commons;
using Application.Interfaces.IRepositories.Clinic;
using Application.Dto;
using Application.Authorization;

/// <summary>
/// Clinic management controller for handling clinic operations
/// </summary>
/// <remarks>
/// This controller manages clinic-related operations including:
/// - Retrieving paginated clinic lists
/// - Individual clinic lookup
/// - Clinic information management
/// - Permission-based access control for different user types
/// </remarks>
[ApiController]
[Route("api/[controller]")]
public class ClinicsController : ControllerBase
{
    private readonly IClinicRepository _repository;

    /// <summary>
    /// Initializes a new instance of the ClinicsController
    /// </summary>
    /// <param name="repository">Clinic repository for data operations</param>
    public ClinicsController(IClinicRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Get paginated list of clinics
    /// </summary>
    /// <remarks>
    /// Retrieves a paginated list of clinics with their information.
    /// This endpoint is typically used by administrators and staff to view clinic records.
    /// 
    /// Required Permission: ViewClinics
    /// </remarks>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="size">Page size (default: 10)</param>
    /// <returns>Paginated list of clinics with their details</returns>
    /// <response code="200">Clinics retrieved successfully</response>
    /// <response code="401">Unauthorized - Missing or invalid token</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("paged")]
    [Permission(Permissions.ViewClinics)]
    [ProducesResponseType(typeof(PagedResult<ClinicListDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<string>), 401)]
    [ProducesResponseType(typeof(ApiResponse<string>), 403)]
    [ProducesResponseType(typeof(ApiResponse<string>), 500)]
    public async Task<ActionResult<PagedResult<ClinicListDto>>> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        try
        {
            var result = await _repository.GetPagedAsync(page, size);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<string>(500, "An error occurred while retrieving clinics.", ex.Message));
        }
    }

    /// <summary>
    /// Get clinic by ID
    /// </summary>
    /// <remarks>
    /// Retrieves detailed information for a specific clinic by its ID.
    /// This includes clinic information, location, and contact details.
    /// 
    /// Required Permission: ViewClinics
    /// </remarks>
    /// <param name="id">Clinic ID</param>
    /// <returns>Clinic details</returns>
    /// <response code="200">Clinic retrieved successfully</response>
    /// <response code="401">Unauthorized - Missing or invalid token</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    /// <response code="404">Clinic not found</response>
    /// <response code="500">Internal server error</response>
    //[HttpGet("{id}")]
    //[Permission(Permissions.ViewClinics)]
    //[ProducesResponseType(typeof(ApiResponse<ClinicListDto>), 200)]
    //[ProducesResponseType(typeof(ApiResponse<string>), 401)]
    //[ProducesResponseType(typeof(ApiResponse<string>), 403)]
    //[ProducesResponseType(typeof(ApiResponse<string>), 404)]
    //[ProducesResponseType(typeof(ApiResponse<string>), 500)]
    //public async Task<ActionResult<ApiResponse<ClinicListDto>>> GetById(long id)
    //{
    //    try
    //    {
    //        var result = await _repository.GetClinicByIdAsync(id);
    //        if (result == null)
    //        {
    //            return NotFound(new ApiResponse<string>(404, "Clinic not found."));
    //        }
    //        return Ok(new ApiResponse<ClinicListDto>(200, "Clinic retrieved successfully.", result));
    //    }
    //    catch (Exception ex)
    //    {
    //        return StatusCode(500, new ApiResponse<string>(500, "An error occurred while retrieving the clinic.", ex.Message));
    //    }
    //}
}
