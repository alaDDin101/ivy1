using Microsoft.AspNetCore.Mvc;
using Application.Commons;
using Application.Dto;
using Application.Interfaces.IRepositories.Specialty;

/// <summary>
/// Medical specialty management controller for handling specialty operations
/// </summary>
/// <remarks>
/// This controller manages medical specialty-related operations including:
/// - Retrieving paginated specialty lists
/// - Individual specialty lookup
/// - Specialty information for doctor classification
/// - No special permissions required (public data)
/// </remarks>
[ApiController]
[Route("api/[controller]")]
public class SpecialtiesController : ControllerBase
{
    private readonly ISpecialtyRepository _repository;

    /// <summary>
    /// Initializes a new instance of the SpecialtiesController
    /// </summary>
    /// <param name="repository">Specialty repository for data operations</param>
    public SpecialtiesController(ISpecialtyRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Get all medical specialties
    /// </summary>
    /// <remarks>
    /// Retrieves all medical specialties. This endpoint is typically used for
    /// doctor classification and patient appointment booking.
    /// No special permissions required as specialty data is considered public.
    /// </remarks>
    /// <returns>List of medical specialties</returns>
    /// <response code="200">Specialties retrieved successfully</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<SpecialtyListDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<string>), 500)]
    public async Task<ActionResult<ApiResponse<List<SpecialtyListDto>>>> GetAll()
    {
        try
        {
            var result = await _repository.GetAllAsync();
            return Ok(new ApiResponse<List<SpecialtyListDto>>(200, "Specialties retrieved successfully.", result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<string>(500, "An error occurred while retrieving specialties.", ex.Message));
        }
    }
}
