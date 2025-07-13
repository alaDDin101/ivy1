using Application.Commons;
using Application.Dto;
using Application.Interfaces.IRepositories;
using Domain.IdentityEntiities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class PermissionService : IPermissionService
    {
        private readonly DataContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public PermissionService(
            DataContext context,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        #region Permission Management

        public async Task<PagedResult<PermissionDto>> GetPermissionsAsync(int page, int size)
        {
            var query = _context.Permissions.AsNoTracking();
            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(p => p.Name)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(p => new PermissionDto
                {
                    Id = p.Id,
                    Name = p.Name
                })
                .ToListAsync();

            return new PagedResult<PermissionDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = size
            };
        }

        public async Task<PermissionDto> GetPermissionByIdAsync(int id)
        {
            var permission = await _context.Permissions
                .FirstOrDefaultAsync(p => p.Id == id);

            if (permission == null)
                throw new Exception("Permission not found");

            return new PermissionDto
            {
                Id = permission.Id,
                Name = permission.Name
            };
        }

        public async Task<PermissionDto> CreatePermissionAsync(CreatePermissionDto dto)
        {
            var existingPermission = await _context.Permissions
                .FirstOrDefaultAsync(p => p.Name == dto.Name);

            if (existingPermission != null)
                throw new Exception("Permission with this name already exists");

            var permission = new Permission
            {
                Name = dto.Name,
            };

            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync();

            return new PermissionDto
            {
                Id = permission.Id,
                Name = permission.Name
            };
        }

        public async Task<PermissionDto> UpdatePermissionAsync(UpdatePermissionDto dto)
        {
            var permission = await _context.Permissions.FindAsync(dto.Id);
            if (permission == null)
                throw new Exception("Permission not found");

            var existingPermission = await _context.Permissions
                .FirstOrDefaultAsync(p => p.Name == dto.Name && p.Id != dto.Id);

            if (existingPermission != null)
                throw new Exception("Permission with this name already exists");

            permission.Name = dto.Name;

            await _context.SaveChangesAsync();

            return new PermissionDto
            {
                Id = permission.Id,
                Name = permission.Name
            };
        }

        public async Task<bool> DeletePermissionAsync(int id)
        {
            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null)
                return false;

            // Check if permission is used by any roles
            var isUsed = await _context.PermissionRoles
                .AnyAsync(pr => pr.PermissionId == id);

            if (isUsed)
                throw new Exception("Cannot delete permission that is assigned to roles");

            _context.Permissions.Remove(permission);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Role Management

        public async Task<PagedResult<RoleDto>> GetRolesAsync(int page, int size)
        {
            var query = _roleManager.Roles.AsNoTracking();
            var totalCount = await query.CountAsync();

            var roles = await query
                .OrderBy(r => r.Name)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            var result = new List<RoleDto>();

            foreach (var role in roles)
            {
                var permissions = await GetRolePermissionsAsync(role.Id);
                result.Add(new RoleDto
                {
                    Id = role.Id,
                    Name = role.Name!,
                    Permissions = permissions
                });
            }

            return new PagedResult<RoleDto>
            {
                Items = result,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = size
            };
        }

        public async Task<RoleDto> GetRoleByIdAsync(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                throw new Exception("Role not found");

            var permissions = await GetRolePermissionsAsync(id);

            return new RoleDto
            {
                Id = role.Id,
                Name = role.Name!,
                Permissions = permissions
            };
        }

        public async Task<RoleDto> CreateRoleAsync(CreateRoleDto dto)
        {
            var existingRole = await _roleManager.FindByNameAsync(dto.Name);
            if (existingRole != null)
                throw new Exception("Role with this name already exists");

            var role = new IdentityRole(dto.Name);
            var result = await _roleManager.CreateAsync(role);

            if (!result.Succeeded)
                throw new Exception("Failed to create role: " + string.Join(", ", result.Errors.Select(e => e.Description)));

            // Assign permissions
            if (dto.PermissionIds.Any())
            {
                await AssignPermissionsToRoleAsync(new AssignPermissionsToRoleDto
                {
                    RoleId = role.Id,
                    PermissionIds = dto.PermissionIds
                });
            }

            return await GetRoleByIdAsync(role.Id);
        }

        public async Task<RoleDto> UpdateRoleAsync(UpdateRoleDto dto)
        {
            var role = await _roleManager.FindByIdAsync(dto.Id);
            if (role == null)
                throw new Exception("Role not found");

            var existingRole = await _roleManager.FindByNameAsync(dto.Name);
            if (existingRole != null && existingRole.Id != dto.Id)
                throw new Exception("Role with this name already exists");

            role.Name = dto.Name;
            var result = await _roleManager.UpdateAsync(role);

            if (!result.Succeeded)
                throw new Exception("Failed to update role: " + string.Join(", ", result.Errors.Select(e => e.Description)));

            // Update permissions
            await AssignPermissionsToRoleAsync(new AssignPermissionsToRoleDto
            {
                RoleId = dto.Id,
                PermissionIds = dto.PermissionIds
            });

            return await GetRoleByIdAsync(dto.Id);
        }

        public async Task<bool> DeleteRoleAsync(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                return false;

            // Check if role is assigned to any users
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
            if (usersInRole.Any())
                throw new Exception("Cannot delete role that is assigned to users");

            var result = await _roleManager.DeleteAsync(role);
            return result.Succeeded;
        }

        #endregion

        #region Permission-Role Management

        public async Task<bool> AssignPermissionsToRoleAsync(AssignPermissionsToRoleDto dto)
        {
            // Remove existing permissions for this role
            var existingPermissions = await _context.PermissionRoles
                .Where(pr => pr.RoleId == dto.RoleId)
                .ToListAsync();

            _context.PermissionRoles.RemoveRange(existingPermissions);

            // Add new permissions
            foreach (var permissionId in dto.PermissionIds)
            {
                var permission = await _context.Permissions.FindAsync(permissionId);
                if (permission != null)
                {
                    _context.PermissionRoles.Add(new PermissionRole
                    {
                        RoleId = dto.RoleId,
                        PermissionId = permissionId
                    });
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<PermissionDto>> GetRolePermissionsAsync(string roleId)
        {
            return await _context.PermissionRoles
                .Where(pr => pr.RoleId == roleId)
                .Include(pr => pr.Permission)
                .Select(pr => new PermissionDto
                {
                    Id = pr.Permission.Id,
                    Name = pr.Permission.Name
                })
                .ToListAsync();
        }

        public async Task<bool> RemovePermissionFromRoleAsync(string roleId, int permissionId)
        {
            var permissionRole = await _context.PermissionRoles
                .FirstOrDefaultAsync(pr => pr.RoleId == roleId && pr.PermissionId == permissionId);

            if (permissionRole == null)
                return false;

            _context.PermissionRoles.Remove(permissionRole);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region User Permission Checking

        public async Task<List<string>> GetUserPermissionsAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new List<string>();

            var userRoles = await _userManager.GetRolesAsync(user);
            var roleIds = await _roleManager.Roles
                .Where(r => userRoles.Contains(r.Name!))
                .Select(r => r.Id)
                .ToListAsync();

            var permissions = await _context.PermissionRoles
                .Where(pr => roleIds.Contains(pr.RoleId))
                .Include(pr => pr.Permission)
                .Select(pr => pr.Permission.Name)
                .Distinct()
                .ToListAsync();

            return permissions;
        }

        public async Task<bool> UserHasPermissionAsync(string userId, string permissionName)
        {
            var userPermissions = await GetUserPermissionsAsync(userId);
            return userPermissions.Contains(permissionName);
        }

        public async Task<UserRoleDto> GetUserRoleInfoAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found");

            var roles = await _userManager.GetRolesAsync(user);
            var permissions = await GetUserPermissionsAsync(userId);

            return new UserRoleDto
            {
                UserId = user.Id,
                Email = user.Email!,
                UserName = user.UserName!,
                Roles = roles.ToList(),
                Permissions = permissions
            };
        }

        #endregion
    }
} 