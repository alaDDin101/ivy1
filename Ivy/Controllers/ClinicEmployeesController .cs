using Application.Commons;
using Application.Dto;
using Application.Exceptions;
using Application.Interfaces.IRepositories.ClinicEmployee;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClinicEmployeesController : ControllerBase
{
    private readonly IClinicEmployeeRepository _repository;

    public ClinicEmployeesController(IClinicEmployeeRepository repository)
    {
        _repository = repository;
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Create([FromBody] CreateClinicEmployeeDto dto)
    {
        try
        {
            var result = await _repository.CreateAsync(dto);
            return Ok(new ApiResponse<ClinicEmployeeDto>(200, "Clinic employee created successfully.", result));
        }
        catch (ValidationException ex)
        {
            return BadRequest(new ApiResponse<string>(400, ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<string>(500, "Unexpected error occurred.", ex.Message));
        }
    }

    [HttpPut("info")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdateInfo([FromBody] UpdateClinicEmployeeInfoDto dto)
    {
        try
        {
            var result = await _repository.UpdateInfoAsync(dto);
            return Ok(new ApiResponse<ClinicEmployeeDto>(200, "Clinic employee info updated successfully.", result));
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ErrorResponse(404, ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ErrorResponse(500, ex.Message));
        }
    }

    [HttpPut("email")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdateEmail([FromBody] UpdateClinicEmployeeEmailDto dto)
    {
        try
        {
            await _repository.UpdateEmailAsync(dto);
            return Ok(new ApiResponse<string>(200, "Email updated successfully."));
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ErrorResponse(404, ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ErrorResponse(500, ex.Message));
        }
    }

    [HttpPut("phone")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdatePhone([FromBody] UpdateClinicEmployeePhoneDto dto)
    {
        try
        {
            await _repository.UpdatePhoneAsync(dto);
            return Ok(new ApiResponse<string>(200, "Phone number updated successfully."));
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ErrorResponse(404, ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ErrorResponse(500, ex.Message));
        }
    }

    [HttpPut("remove/{personId:long}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> RemoveFromClinic(long personId)
    {
        try
        {
            await _repository.RemoveFromClinicAsync(personId);
            return Ok(new ApiResponse<string>(200, "Clinic employee removed successfully."));
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ErrorResponse(404, ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ErrorResponse(500, ex.Message));
        }
    }

    [HttpGet("paged")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        var result = await _repository.GetPagedAsync(page, size);
        return Ok(result);
    }
}
