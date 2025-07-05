using Application.Commons;
using Application.Dto;
using Application.Interfaces.IRepositories.City;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CitiesController : ControllerBase
    {
        private readonly ICityRepository _cityRepository;

        public CitiesController(ICityRepository cityRepository)
        {
            _cityRepository = cityRepository;
        }

        [HttpGet("paged")]
        [Authorize]
        public async Task<ActionResult<PagedResult<CityListDto>>> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            var result = await _cityRepository.GetPagedAsync(page, size);
            return Ok(new ApiResponse<PagedResult<CityListDto>>(200,"", result));
        }

        [HttpPost]
        public async Task<ActionResult<CityListDto>> Create([FromBody] CreateCityDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _cityRepository.CreateAsync(dto);
            return Ok(new ApiResponse<CityListDto>(200, "City created successfully.", result));
        }

        [HttpPut]
        public async Task<ActionResult<CityListDto>> Update([FromBody] UpdateCityDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _cityRepository.UpdateAsync(dto);
                return Ok(new ApiResponse<CityListDto>(200, "City updated successfully.", result));
            }
            catch (Exception ex)
            {
                return NotFound(new ErrorResponse(404, ex.Message));
            }
        }
    }
}
