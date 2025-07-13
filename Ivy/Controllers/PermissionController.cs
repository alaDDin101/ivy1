using Microsoft.AspNetCore.Mvc;
using Application.Commons;
using Application.Interfaces.IRepositories;
using Application.Dto;
using Application.Authorization;

/// <summary>
/// Permission management controller for handling system permissions
/// </summary>
/// <remarks>
/// This controller manages permission-related operations including:
/// - Creating, updating, and deleting permissions
/// - Retrieving permission information
/// - Administrative operations for permission-based access control
/// - Fine-grained access control for system resources
/// - Requires high-level admin permissions
/// </remarks>
[ApiController]
[Route("api/[controller]")]
public class PermissionController : ControllerBase
{
    private readonly IPermissionService _permissionService;

    /// <summary>
    /// Initializes a new instance of the PermissionController
    /// </summary>
    /// <param name="permissionService">Permission service for permission operations</param>
    public PermissionController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    /// <summary>
    /// Get paginated permissions
    /// </summary>
    /// <remarks>
    /// Retrieves paginated permissions in the system with their basic information.
    /// This endpoint is typically used by administrators to view and manage permissions.
    /// 
    /// Required Permission: ViewPermissions
    /// </remarks>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="size">Page size (default: 10)</param>
    /// <returns>Paginated list of permissions</returns>
    /// <response code="200">Permissions retrieved successfully</response>
    /// <response code="401">Unauthorized - Missing or invalid token</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [Permission(Permissions.ViewPermissions)]
    [ProducesResponseType(typeof(PagedResult<PermissionDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<string>), 401)]
    [ProducesResponseType(typeof(ApiResponse<string>), 403)]
    [ProducesResponseType(typeof(ApiResponse<string>), 500)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        try
        {
            var permissions = await _permissionService.GetPermissionsAsync(page, size);
            return Ok(permissions);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<string>(500, "An error occurred while retrieving permissions.", ex.Message));
        }
    }

    /// <summary>
    /// Get permission by ID
    /// </summary>
    /// <remarks>
    /// Retrieves detailed information for a specific permission by its ID.
    /// This includes the permission's name, description, and usage information.
    /// 
    /// Required Permission: ViewPermissions
    /// </remarks>
    /// <param name="id">Permission ID</param>
    /// <returns>Permission details</returns>
    /// <response code="200">Permission retrieved successfully</response>
    /// <response code="401">Unauthorized - Missing or invalid token</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    /// <response code="404">Permission not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id}")]
    [Permission(Permissions.ViewPermissions)]
    [ProducesResponseType(typeof(ApiResponse<PermissionDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<string>), 401)]
    [ProducesResponseType(typeof(ApiResponse<string>), 403)]
    [ProducesResponseType(typeof(ApiResponse<string>), 404)]
    [ProducesResponseType(typeof(ApiResponse<string>), 500)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var permission = await _permissionService.GetPermissionByIdAsync(id);
            if (permission == null)
            {
                return NotFound(new ApiResponse<string>(404, "Permission not found."));
            }
            return Ok(new ApiResponse<PermissionDto>(200, "Permission retrieved successfully.", permission));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<string>(500, "An error occurred while retrieving the permission.", ex.Message));
        }
    }

    /// <summary>
    /// Create a new permission
    /// </summary>
    /// <remarks>
    /// Creates a new permission in the system. Permissions define what actions
    /// users can perform on specific resources.
    /// 
    /// Required Permission: CreatePermissions
    /// </remarks>
    /// <param name="dto">Permission creation details</param>
    /// <returns>Created permission details</returns>
    /// <response code="201">Permission created successfully</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Unauthorized - Missing or invalid token</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [Permission(Permissions.CreatePermissions)]
    [ProducesResponseType(typeof(ApiResponse<PermissionDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse<string>), 400)]
    [ProducesResponseType(typeof(ApiResponse<string>), 401)]
    [ProducesResponseType(typeof(ApiResponse<string>), 403)]
    [ProducesResponseType(typeof(ApiResponse<string>), 500)]
    public async Task<IActionResult> Create([FromBody] CreatePermissionDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<string>(400, "Invalid input data."));
            }

            var permission = await _permissionService.CreatePermissionAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = permission.Id }, 
                new ApiResponse<PermissionDto>(201, "Permission created successfully.", permission));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<string>(500, "An error occurred while creating the permission.", ex.Message));
        }
    }

    /// <summary>
    /// Update an existing permission
    /// </summary>
    /// <remarks>
    /// Updates permission information such as name and description.
    /// Be careful when modifying permissions as this may affect access control.
    /// 
    /// Required Permission: UpdatePermissions
    /// </remarks>
    /// <param name="dto">Updated permission details</param>
    /// <returns>Updated permission details</returns>
    /// <response code="200">Permission updated successfully</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Unauthorized - Missing or invalid token</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    /// <response code="404">Permission not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut]
    [Permission(Permissions.UpdatePermissions)]
    [ProducesResponseType(typeof(ApiResponse<PermissionDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<string>), 400)]
    [ProducesResponseType(typeof(ApiResponse<string>), 401)]
    [ProducesResponseType(typeof(ApiResponse<string>), 403)]
    [ProducesResponseType(typeof(ApiResponse<string>), 404)]
    [ProducesResponseType(typeof(ApiResponse<string>), 500)]
    public async Task<IActionResult> Update([FromBody] UpdatePermissionDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<string>(400, "Invalid input data."));
            }

            var permission = await _permissionService.UpdatePermissionAsync(dto);
            return Ok(new ApiResponse<PermissionDto>(200, "Permission updated successfully.", permission));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<string>(500, "An error occurred while updating the permission.", ex.Message));
        }
    }

    /// <summary>
    /// Delete a permission
    /// </summary>
    /// <remarks>
    /// Deletes a permission from the system. This will remove all role assignments
    /// for this permission and may affect access control throughout the system.
    /// Use with extreme caution.
    /// 
    /// Required Permission: DeletePermissions
    /// </remarks>
    /// <param name="id">Permission ID to delete</param>
    /// <returns>Deletion confirmation</returns>
    /// <response code="200">Permission deleted successfully</response>
    /// <response code="401">Unauthorized - Missing or invalid token</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    /// <response code="404">Permission not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{id}")]
    [Permission(Permissions.DeletePermissions)]
    [ProducesResponseType(typeof(ApiResponse<string>), 200)]
    [ProducesResponseType(typeof(ApiResponse<string>), 401)]
    [ProducesResponseType(typeof(ApiResponse<string>), 403)]
    [ProducesResponseType(typeof(ApiResponse<string>), 404)]
    [ProducesResponseType(typeof(ApiResponse<string>), 500)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _permissionService.DeletePermissionAsync(id);
            return Ok(new ApiResponse<string>(200, "Permission deleted successfully."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<string>(500, "An error occurred while deleting the permission.", ex.Message));
        }
    }
} 