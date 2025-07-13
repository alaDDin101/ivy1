using Microsoft.AspNetCore.Mvc;
using Application.Commons;
using Application.Interfaces.IRepositories.City;
using Application.Dto;

/// <summary>
/// City management controller for handling city operations
/// </summary>
/// <remarks>
/// This controller manages city-related operations including:
/// - Retrieving paginated city lists
/// - Individual city lookup
/// - City information for address management
/// - No special permissions required (public data)
/// </remarks>
[ApiController]
[Route("api/[controller]")]
public class CitiesController : ControllerBase
{
    private readonly ICityRepository _repository;

    /// <summary>
    /// Initializes a new instance of the CitiesController
    /// </summary>
    /// <param name="repository">City repository for data operations</param>
    public CitiesController(ICityRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Get paginated list of cities
    /// </summary>
    /// <remarks>
    /// Retrieves a paginated list of cities. This endpoint is typically used for
    /// address selection in patient registration and other forms.
    /// No special permissions required as city data is considered public.
    /// </remarks>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="size">Page size (default: 10)</param>
    /// <returns>Paginated list of cities</returns>
    /// <response code="200">Cities retrieved successfully</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("paged")]
    [ProducesResponseType(typeof(PagedResult<CityListDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<string>), 500)]
    public async Task<ActionResult<PagedResult<CityListDto>>> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        try
        {
            var result = await _repository.GetPagedAsync(page, size);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<string>(500, "An error occurred while retrieving cities.", ex.Message));
        }
    }

    /// <summary>
    /// Get city by ID
    /// </summary>
    /// <remarks>
    /// Retrieves detailed information for a specific city by its ID.
    /// This includes city name and related information.
    /// No special permissions required as city data is considered public.
    /// </remarks>
    /// <param name="id">City ID</param>
    /// <returns>City details</returns>
    /// <response code="200">City retrieved successfully</response>
    /// <response code="404">City not found</response>
    /// <response code="500">Internal server error</response>
    //[HttpGet("{id}")]
    //[ProducesResponseType(typeof(ApiResponse<CityListDto>), 200)]
    //[ProducesResponseType(typeof(ApiResponse<string>), 404)]
    //[ProducesResponseType(typeof(ApiResponse<string>), 500)]
    //public async Task<ActionResult<ApiResponse<CityListDto>>> GetById(long id)
    //{
    //    try
    //    {
    //        var result = await _repository.g(id);
    //        if (result == null)
    //        {
    //            return NotFound(new ApiResponse<string>(404, "City not found."));
    //        }
    //        return Ok(new ApiResponse<CityListDto>(200, "City retrieved successfully.", result));
    //    }
    //    catch (Exception ex)
    //    {
    //        return StatusCode(500, new ApiResponse<string>(500, "An error occurred while retrieving the city.", ex.Message));
    //    }
    //}
}
