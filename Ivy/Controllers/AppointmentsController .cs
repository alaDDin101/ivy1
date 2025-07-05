using Microsoft.AspNetCore.Mvc;
using Application.Commons;
using Application.Interfaces.IRepositories.Appointment;
using Application.Dto;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentRepository _repository;

    public AppointmentsController(IAppointmentRepository repository)
    {
        _repository = repository;
    }

    [HttpPost]
    public async Task<IActionResult> Book([FromBody] CreateAppointmentDto dto)
    {
        var result = await _repository.BookAppointmentAsync(dto);
        return Ok(new ApiResponse<AppointmentListDto>(200, "Appointment booked successfully.", result));
    }

    [HttpGet("paged")]
    public async Task<ActionResult<PagedResult<AppointmentListDto>>> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        var result = await _repository.GetPagedAppointmentsAsync(page, size);
        return Ok(result);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateAppointmentDto dto)
    {
        var result = await _repository.UpdateAppointmentAsync(dto);
        return Ok(new ApiResponse<AppointmentListDto>(200, "Appointment updated successfully.", result));
    }
}
