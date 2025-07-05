using Microsoft.AspNetCore.Mvc;
using Application.Commons;
using Application.Interfaces.IRepositories.Clinic;
using Application.Dto;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClinicsController : ControllerBase
    {
        private readonly IClinicRepository _repository;

        public ClinicsController(IClinicRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("paged")]
        public async Task<ActionResult<PagedResult<ClinicListDto>>> GetPaged(
            [FromQuery] int page = 1,
            [FromQuery] int size = 10)
        {
            var result = await _repository.GetPagedAsync(page, size);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<ClinicListDto>>> Create([FromBody] CreateClinicDto dto)
        {
            try
            {
                var result = await _repository.CreateClinicAsync(dto);
                return Ok(new ApiResponse<ClinicListDto>(200, "Clinic created successfully", result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(500, "An error occurred while creating the clinic.", ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<ClinicListDto>>> Update(long id, [FromBody] UpdateClinicDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest(new ApiResponse<string>(400, "ID in URL does not match ID in body."));
            }

            try
            {
                var result = await _repository.UpdateClinicAsync(dto);
                return Ok(new ApiResponse<ClinicListDto>(200, "Clinic updated successfully", result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(500, "An error occurred while updating the clinic.", ex.Message));
            }
        }
    }
}
