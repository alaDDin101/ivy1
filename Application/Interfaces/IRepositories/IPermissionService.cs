using Application.Commons;
using Application.Dto;

namespace Application.Interfaces.IRepositories
{
    public interface IPermissionService
    {
        // Permission Management
        Task<PagedResult<PermissionDto>> GetPermissionsAsync(int page, int size);
        Task<PermissionDto> GetPermissionByIdAsync(int id);
        Task<PermissionDto> CreatePermissionAsync(CreatePermissionDto dto);
        Task<PermissionDto> UpdatePermissionAsync(UpdatePermissionDto dto);
        Task<bool> DeletePermissionAsync(int id);

        // Role Management
        Task<PagedResult<RoleDto>> GetRolesAsync(int page, int size);
        Task<RoleDto> GetRoleByIdAsync(string id);
        Task<RoleDto> CreateRoleAsync(CreateRoleDto dto);
        Task<RoleDto> UpdateRoleAsync(UpdateRoleDto dto);
        Task<bool> DeleteRoleAsync(string id);

        // Permission-Role Management
        Task<bool> AssignPermissionsToRoleAsync(AssignPermissionsToRoleDto dto);
        Task<List<PermissionDto>> GetRolePermissionsAsync(string roleId);
        Task<bool> RemovePermissionFromRoleAsync(string roleId, int permissionId);

        // User Permission Checking
        Task<List<string>> GetUserPermissionsAsync(string userId);
        Task<bool> UserHasPermissionAsync(string userId, string permissionName);
        Task<UserRoleDto> GetUserRoleInfoAsync(string userId);
    }
} 