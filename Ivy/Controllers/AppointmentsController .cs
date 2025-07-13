using Microsoft.AspNetCore.Mvc;
using Application.Commons;
using Application.Interfaces.IRepositories.Appointment;
using Application.Dto;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Application.Authorization;

/// <summary>
/// Appointment management controller for handling appointment booking, scheduling, and management
/// </summary>
/// <remarks>
/// This controller manages the complete appointment workflow including:
/// - Multi-step appointment booking process (Patient request → Staff acceptance → Patient confirmation)
/// - Role-based appointment viewing and management
/// - Appointment status transitions (Pending → Waiting for Confirmation → Scheduled)
/// - Permission-based access control for different user types
/// </remarks>
[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentRepository _repository;

    /// <summary>
    /// Initializes a new instance of the AppointmentsController
    /// </summary>
    /// <param name="repository">Appointment repository for data operations</param>
    public AppointmentsController(IAppointmentRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Book an appointment directly by clinic staff (bypasses patient workflow)
    /// </summary>
    /// <remarks>
    /// Allows clinic staff to book appointments directly with confirmed date and time.
    /// This endpoint bypasses the normal patient request workflow and creates a confirmed appointment.
    /// 
    /// Required Permission: BookAppointments
    /// </remarks>
    /// <param name="dto">Appointment booking details including patient, doctor, date, and reason</param>
    /// <returns>Created appointment details</returns>
    /// <response code="200">Appointment booked successfully</response>
    /// <response code="401">Unauthorized - Missing or invalid token</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("book-by-staff")]
    [Permission(Permissions.BookAppointments)]
    [ProducesResponseType(typeof(ApiResponse<AppointmentListDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<string>), 401)]
    [ProducesResponseType(typeof(ApiResponse<string>), 403)]
    [ProducesResponseType(typeof(ApiResponse<string>), 500)]
    public async Task<IActionResult> BookByStaff([FromBody] CreateAppointmentDto dto)
    {
        try
        {
            var result = await _repository.BookAppointmentAsync(dto);
            return Ok(new ApiResponse<AppointmentListDto>(200, "Appointment booked successfully by staff.", result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<string>(500, "An error occurred while booking the appointment.", ex.Message));
        }
    }

    /// <summary>
    /// Submit appointment request by patient (Step 1 of appointment workflow)
    /// </summary>
    /// <remarks>
    /// Allows patients to submit appointment requests without specifying a date.
    /// The appointment is created with:
    /// - Status: Pending (awaiting staff acceptance)
    /// - Date: null (to be proposed by staff)
    /// - Reason: provided by patient
    /// 
    /// Workflow: Patient Request → Staff Acceptance → Patient Confirmation
    /// Required Permission: BookAppointments
    /// </remarks>
    /// <param name="dto">Patient appointment request details</param>
    /// <returns>Created appointment request details</returns>
    /// <response code="200">Appointment request submitted successfully</response>
    /// <response code="401">Unauthorized - Missing or invalid token</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("book-by-patient")]
    [Permission(Permissions.BookAppointments)]
    [ProducesResponseType(typeof(ApiResponse<AppointmentListDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<string>), 401)]
    [ProducesResponseType(typeof(ApiResponse<string>), 403)]
    [ProducesResponseType(typeof(ApiResponse<string>), 500)]
    public async Task<IActionResult> BookByPatient([FromBody] CreateAppointmentByPatientDto dto)
    {
        try
        {
            var result = await _repository.BookAppointmentByPatientAsync(dto);
            return Ok(new ApiResponse<AppointmentListDto>(200, "Appointment request submitted successfully.", result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<string>(500, "An error occurred while submitting the appointment request.", ex.Message));
        }
    }

    /// <summary>
    /// Accept appointment request and propose date (Step 2 of appointment workflow)
    /// </summary>
    /// <remarks>
    /// Allows clinic staff to accept patient appointment requests and propose a date/time.
    /// The appointment status changes from Pending to WaitingForPatientConfirmation.
    /// 
    /// Workflow: Patient Request → **Staff Acceptance** → Patient Confirmation
    /// Required Permission: UpdateAppointments
    /// </remarks>
    /// <param name="dto">Staff acceptance details including appointment ID and proposed date</param>
    /// <returns>Updated appointment details with proposed date</returns>
    /// <response code="200">Appointment accepted and date proposed</response>
    /// <response code="401">Unauthorized - Missing or invalid token</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    /// <response code="404">Appointment not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("accept-by-staff")]
    [Permission(Permissions.UpdateAppointments)]
    [ProducesResponseType(typeof(ApiResponse<AppointmentListDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<string>), 401)]
    [ProducesResponseType(typeof(ApiResponse<string>), 403)]
    [ProducesResponseType(typeof(ApiResponse<string>), 404)]
    [ProducesResponseType(typeof(ApiResponse<string>), 500)]
    public async Task<IActionResult> AcceptByStaff([FromBody] AcceptAppointmentByStaffDto dto)
    {
        try
        {
            var result = await _repository.AcceptAppointmentByStaffAsync(dto.AppointmentId, dto.ProposedDate);
            return Ok(new ApiResponse<AppointmentListDto>(200, "Appointment accepted and date proposed.", result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<string>(500, "An error occurred while accepting the appointment.", ex.Message));
        }
    }

    /// <summary>
    /// Confirm or reject appointment by patient (Step 3 of appointment workflow)
    /// </summary>
    /// <remarks>
    /// Allows patients to confirm or reject the date/time proposed by clinic staff.
    /// - If accepted: Status changes to Scheduled
    /// - If rejected: Status returns to Pending and date is removed
    /// 
    /// Workflow: Patient Request → Staff Acceptance → **Patient Confirmation**
    /// Required Permission: ConfirmAppointments
    /// </remarks>
    /// <param name="dto">Patient confirmation details including appointment ID and acceptance decision</param>
    /// <returns>Updated appointment details with final status</returns>
    /// <response code="200">Appointment confirmed successfully or rejected and returned to pending</response>
    /// <response code="401">Unauthorized - Missing or invalid token</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    /// <response code="404">Appointment not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("confirm-by-patient")]
    [Permission(Permissions.ConfirmAppointments)]
    [ProducesResponseType(typeof(ApiResponse<AppointmentListDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<string>), 401)]
    [ProducesResponseType(typeof(ApiResponse<string>), 403)]
    [ProducesResponseType(typeof(ApiResponse<string>), 404)]
    [ProducesResponseType(typeof(ApiResponse<string>), 500)]
    public async Task<IActionResult> ConfirmByPatient([FromBody] ConfirmAppointmentByPatientDto dto)
    {
        try
        {
            var result = await _repository.ConfirmAppointmentByPatientAsync(dto.AppointmentId, dto.IsAccepted);
            var message = dto.IsAccepted ? "Appointment confirmed successfully." : "Appointment rejected and returned to pending.";
            return Ok(new ApiResponse<AppointmentListDto>(200, message, result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<string>(500, "An error occurred while confirming the appointment.", ex.Message));
        }
    }

    /// <summary>
    /// Get paginated appointments with role-based filtering
    /// </summary>
    /// <remarks>
    /// Retrieves appointments with automatic role-based filtering:
    /// - **Patients**: See only their own appointments
    /// - **Doctors**: See appointments for their doctor-clinic associations
    /// - **Clinic Staff**: See appointments for their clinic
    /// - **Admins**: See all appointments
    /// 
    /// Required Permission: ViewAppointments
    /// </remarks>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="size">Page size (default: 10)</param>
    /// <returns>Paginated list of appointments based on user role</returns>
    /// <response code="200">Appointments retrieved successfully</response>
    /// <response code="401">Unauthorized - Missing or invalid token</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("paged")]
    [Permission(Permissions.ViewAppointments)]
    [ProducesResponseType(typeof(PagedResult<AppointmentListDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<string>), 401)]
    [ProducesResponseType(typeof(ApiResponse<string>), 403)]
    [ProducesResponseType(typeof(ApiResponse<string>), 500)]
    public async Task<ActionResult<PagedResult<AppointmentListDto>>> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new ApiResponse<string>(401, "Invalid user context."));

            var result = await _repository.GetPagedAppointmentsAsync(page, size, userId, roles);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<string>(500, "An error occurred while retrieving appointments.", ex.Message));
        }
    }

    /// <summary>
    /// Update appointment details
    /// </summary>
    /// <remarks>
    /// Allows updating appointment information including date, reason, and status.
    /// This is typically used for administrative changes or rescheduling.
    /// 
    /// Required Permission: UpdateAppointments
    /// </remarks>
    /// <param name="dto">Updated appointment details</param>
    /// <returns>Updated appointment details</returns>
    /// <response code="200">Appointment updated successfully</response>
    /// <response code="401">Unauthorized - Missing or invalid token</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    /// <response code="404">Appointment not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut]
    [Permission(Permissions.UpdateAppointments)]
    [ProducesResponseType(typeof(ApiResponse<AppointmentListDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<string>), 401)]
    [ProducesResponseType(typeof(ApiResponse<string>), 403)]
    [ProducesResponseType(typeof(ApiResponse<string>), 404)]
    [ProducesResponseType(typeof(ApiResponse<string>), 500)]
    public async Task<IActionResult> Update([FromBody] UpdateAppointmentDto dto)
    {
        try
        {
            var result = await _repository.UpdateAppointmentAsync(dto);
            return Ok(new ApiResponse<AppointmentListDto>(200, "Appointment updated successfully.", result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<string>(500, "An error occurred while updating the appointment.", ex.Message));
        }
    }
}
