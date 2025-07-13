using Microsoft.AspNetCore.Mvc;
using Application.Commons;
using Application.Interfaces.IRepositories;
using Application.Dto;
using Application.Authorization;

/// <summary>
/// Role management controller for handling user roles and permissions
/// </summary>
/// <remarks>
/// This controller manages role-related operations including:
/// - Creating, updating, and deleting roles
/// - Assigning permissions to roles
/// - Retrieving role information
/// - Administrative operations for role-based access control
/// - Requires high-level admin permissions
/// </remarks>
[ApiController]
[Route("api/[controller]")]
public class RoleController : ControllerBase
{
    private readonly IPermissionService _permissionService;

    /// <summary>
    /// Initializes a new instance of the RoleController
    /// </summary>
    /// <param name="permissionService">Permission service for role and permission operations</param>
    public RoleController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    /// <summary>
    /// Get paginated roles
    /// </summary>
    /// <remarks>
    /// Retrieves paginated roles in the system with their basic information.
    /// This endpoint is typically used by administrators to view and manage roles.
    /// 
    /// Required Permission: ViewRoles
    /// </remarks>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="size">Page size (default: 10)</param>
    /// <returns>Paginated list of roles</returns>
    /// <response code="200">Roles retrieved successfully</response>
    /// <response code="401">Unauthorized - Missing or invalid token</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [Permission(Permissions.ViewRoles)]
    [ProducesResponseType(typeof(PagedResult<RoleDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<string>), 401)]
    [ProducesResponseType(typeof(ApiResponse<string>), 403)]
    [ProducesResponseType(typeof(ApiResponse<string>), 500)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        try
        {
            var roles = await _permissionService.GetRolesAsync(page, size);
            return Ok(roles);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<string>(500, "An error occurred while retrieving roles.", ex.Message));
        }
    }

    /// <summary>
    /// Get role by ID
    /// </summary>
    /// <remarks>
    /// Retrieves detailed information for a specific role by its ID.
    /// This includes the role's permissions and other metadata.
    /// 
    /// Required Permission: ViewRoles
    /// </remarks>
    /// <param name="id">Role ID</param>
    /// <returns>Role details</returns>
    /// <response code="200">Role retrieved successfully</response>
    /// <response code="401">Unauthorized - Missing or invalid token</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    /// <response code="404">Role not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id}")]
    [Permission(Permissions.ViewRoles)]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<string>), 401)]
    [ProducesResponseType(typeof(ApiResponse<string>), 403)]
    [ProducesResponseType(typeof(ApiResponse<string>), 404)]
    [ProducesResponseType(typeof(ApiResponse<string>), 500)]
    public async Task<IActionResult> GetById(string id)
    {
        try
        {
            var role = await _permissionService.GetRoleByIdAsync(id);
            if (role == null)
            {
                return NotFound(new ApiResponse<string>(404, "Role not found."));
            }
            return Ok(new ApiResponse<RoleDto>(200, "Role retrieved successfully.", role));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<string>(500, "An error occurred while retrieving the role.", ex.Message));
        }
    }

    /// <summary>
    /// Create a new role
    /// </summary>
    /// <remarks>
    /// Creates a new role in the system. Roles are used to group permissions
    /// and can be assigned to users for access control.
    /// 
    /// Required Permission: CreateRoles
    /// </remarks>
    /// <param name="dto">Role creation details</param>
    /// <returns>Created role details</returns>
    /// <response code="201">Role created successfully</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Unauthorized - Missing or invalid token</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [Permission(Permissions.CreateRoles)]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse<string>), 400)]
    [ProducesResponseType(typeof(ApiResponse<string>), 401)]
    [ProducesResponseType(typeof(ApiResponse<string>), 403)]
    [ProducesResponseType(typeof(ApiResponse<string>), 500)]
    public async Task<IActionResult> Create([FromBody] CreateRoleDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<string>(400, "Invalid input data."));
            }

            var role = await _permissionService.CreateRoleAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = role.Id }, 
                new ApiResponse<RoleDto>(201, "Role created successfully.", role));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<string>(500, "An error occurred while creating the role.", ex.Message));
        }
    }

    /// <summary>
    /// Update an existing role
    /// </summary>
    /// <remarks>
    /// Updates role information such as name and description.
    /// Note: Use the assign-permissions endpoint to modify role permissions.
    /// 
    /// Required Permission: UpdateRoles
    /// </remarks>
    /// <param name="dto">Updated role details</param>
    /// <returns>Updated role details</returns>
    /// <response code="200">Role updated successfully</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Unauthorized - Missing or invalid token</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    /// <response code="404">Role not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut]
    [Permission(Permissions.UpdateRoles)]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<string>), 400)]
    [ProducesResponseType(typeof(ApiResponse<string>), 401)]
    [ProducesResponseType(typeof(ApiResponse<string>), 403)]
    [ProducesResponseType(typeof(ApiResponse<string>), 404)]
    [ProducesResponseType(typeof(ApiResponse<string>), 500)]
    public async Task<IActionResult> Update([FromBody] UpdateRoleDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<string>(400, "Invalid input data."));
            }

            var role = await _permissionService.UpdateRoleAsync(dto);
            return Ok(new ApiResponse<RoleDto>(200, "Role updated successfully.", role));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<string>(500, "An error occurred while updating the role.", ex.Message));
        }
    }

    /// <summary>
    /// Delete a role
    /// </summary>
    /// <remarks>
    /// Deletes a role from the system. This will remove all permission assignments
    /// for this role and may affect users who have this role assigned.
    /// 
    /// Required Permission: DeleteRoles
    /// </remarks>
    /// <param name="id">Role ID to delete</param>
    /// <returns>Deletion confirmation</returns>
    /// <response code="200">Role deleted successfully</response>
    /// <response code="401">Unauthorized - Missing or invalid token</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    /// <response code="404">Role not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{id}")]
    [Permission(Permissions.DeleteRoles)]
    [ProducesResponseType(typeof(ApiResponse<string>), 200)]
    [ProducesResponseType(typeof(ApiResponse<string>), 401)]
    [ProducesResponseType(typeof(ApiResponse<string>), 403)]
    [ProducesResponseType(typeof(ApiResponse<string>), 404)]
    [ProducesResponseType(typeof(ApiResponse<string>), 500)]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            await _permissionService.DeleteRoleAsync(id);
            return Ok(new ApiResponse<string>(200, "Role deleted successfully."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<string>(500, "An error occurred while deleting the role.", ex.Message));
        }
    }

    /// <summary>
    /// Assign permissions to a role
    /// </summary>
    /// <remarks>
    /// Assigns multiple permissions to a role. This replaces all existing
    /// permissions for the role with the provided list.
    /// 
    /// Required Permission: ManageRolePermissions
    /// </remarks>
    /// <param name="dto">Role ID and list of permission IDs to assign</param>
    /// <returns>Assignment confirmation</returns>
    /// <response code="200">Permissions assigned successfully</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Unauthorized - Missing or invalid token</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    /// <response code="404">Role or permissions not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("assign-permissions")]
    [Permission(Permissions.ManageRolePermissions)]
    [ProducesResponseType(typeof(ApiResponse<string>), 200)]
    [ProducesResponseType(typeof(ApiResponse<string>), 400)]
    [ProducesResponseType(typeof(ApiResponse<string>), 401)]
    [ProducesResponseType(typeof(ApiResponse<string>), 403)]
    [ProducesResponseType(typeof(ApiResponse<string>), 404)]
    [ProducesResponseType(typeof(ApiResponse<string>), 500)]
    public async Task<IActionResult> AssignPermissions([FromBody] AssignPermissionsToRoleDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<string>(400, "Invalid input data."));
            }

            await _permissionService.AssignPermissionsToRoleAsync(dto);
            return Ok(new ApiResponse<string>(200, "Permissions assigned to role successfully."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<string>(500, "An error occurred while assigning permissions.", ex.Message));
        }
    }

    /// <summary>
    /// Get permissions assigned to a role
    /// </summary>
    /// <remarks>
    /// Retrieves all permissions currently assigned to a specific role.
    /// This is useful for viewing and managing role-based access control.
    /// 
    /// Required Permission: ViewRoles
    /// </remarks>
    /// <param name="roleId">Role ID</param>
    /// <returns>List of permissions assigned to the role</returns>
    /// <response code="200">Role permissions retrieved successfully</response>
    /// <response code="401">Unauthorized - Missing or invalid token</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    /// <response code="404">Role not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{roleId}/permissions")]
    [Permission(Permissions.ViewRoles)]
    [ProducesResponseType(typeof(ApiResponse<List<PermissionDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<string>), 401)]
    [ProducesResponseType(typeof(ApiResponse<string>), 403)]
    [ProducesResponseType(typeof(ApiResponse<string>), 404)]
    [ProducesResponseType(typeof(ApiResponse<string>), 500)]
    public async Task<IActionResult> GetRolePermissions(string roleId)
    {
        try
        {
            var permissions = await _permissionService.GetRolePermissionsAsync(roleId);
            return Ok(new ApiResponse<List<PermissionDto>>(200, "Role permissions retrieved successfully.", permissions));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<string>(500, "An error occurred while retrieving role permissions.", ex.Message));
        }
    }
} 