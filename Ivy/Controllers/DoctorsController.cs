using Microsoft.AspNetCore.Mvc;
using Application.Dto;
using Application.Commons;
using Application.Interfaces.IRepositories.Doctor;
using Application.Exceptions;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Ivy.Infrastructure.Repositories;

[ApiController]
[Route("api/[controller]")]
public class DoctorsController : ControllerBase
{
    private readonly IDoctorRepository _repository;

    public DoctorsController(IDoctorRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("paged")]
    public async Task<ActionResult<PagedResult<DoctorDto>>> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        var result = await _repository.GetPagedAsync(page, size);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "admin")]    
    public async Task<IActionResult> Create([FromBody] CreateDoctorDto dto)
    {
        try
        {
            var result = await _repository.CreateAsync(dto);
            return Ok(new ApiResponse<DoctorDto>(200, "Doctor created successfully.", result));
        }
        catch (ValidationException ex)
        {
            return BadRequest(new ApiResponse<string>(400, ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<string>(500, "Unexpected error", ex.Message));
        }
    }

    //[HttpPut("{id}")]
    //public async Task<IActionResult> Update(long id, [FromBody] DoctorDto dto)
    //{
    //    if (id != dto.Id)
    //        return BadRequest(new ApiResponse<string>(400, "ID mismatch."));

    //    try
    //    {
    //        var result = await _repository.UpdateAsync(dto);
    //        return Ok(new ApiResponse<DoctorDto>(200, "Doctor updated successfully.", result));
    //    }
    //    catch (NotFoundException ex)
    //    {
    //        return NotFound(new ApiResponse<string>(404, ex.Message));
    //    }
    //    catch (ValidationException ex)
    //    {
    //        return BadRequest(new ApiResponse<string>(400, ex.Message));
    //    }
    //    catch (Exception ex)
    //    {
    //        return StatusCode(500, new ApiResponse<string>(500, "Unexpected error", ex.Message));
    //    }
    //}
    [HttpPut("info")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> EditInfo([FromBody] EditDoctorInfoDto dto)
    {
        try
        {
            await _repository.EditDoctorInfoAsync(dto);
            return Ok(new ApiResponse<string>(200, "Doctor information updated successfully."));
        }
        catch (Exception ex)
        {
            return NotFound(new ErrorResponse(404, ex.Message));
        }
    }

    [HttpPut("clinic")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> EditClinic([FromBody] EditDoctorClinicDto dto)
    {
        try
        {
            await _repository.EditDoctorClinicAsync(dto);
            return Ok(new ApiResponse<string>(200, "Doctor clinic updated successfully."));
        }
        catch (Exception ex)
        {
            return NotFound(new ErrorResponse(404, ex.Message));
        }
    }

    [HttpPut("phone")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> EditPhone([FromBody] EditDoctorPhoneDto dto)
    {
        try
        {
            await _repository.EditDoctorPhoneAsync(dto);
            return Ok(new ApiResponse<string>(200, "Doctor phone number updated successfully."));
        }
        catch (Exception ex)
        {
            return NotFound(new ErrorResponse(404, ex.Message));
        }
    }

    [HttpPut("email")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> EditEmail([FromBody] EditDoctorEmailDto dto)
    {
        try
        {
            await _repository.EditDoctorEmailAsync(dto);
            return Ok(new ApiResponse<string>(200, "Doctor email updated successfully."));
        }
        catch (Exception ex)
        {
            return NotFound(new ErrorResponse(404, ex.Message));
        }
    }
}
