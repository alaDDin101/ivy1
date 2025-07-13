using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Application.Dto
{
    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
    public class LoginResponseDto
    {
        public string Email { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FatherName { get; set; }
        public string NationalNumber { get; set; }
        public DateOnly? BirthDate { get; set; }
        public string Role { get; set; }
        public string Token { get; set; }
    }
    public class RegisterDto
    {
        [Required]
        public string FirstName { get; set; } = null!;
        
        [Required]
        public string FatherName { get; set; } = null!;
        
        [Required]
        public string LastName { get; set; } = null!;
        
        public string? MotherName { get; set; }
        
        [Required]
        public DateOnly BirthDate { get; set; }
        
        public string? Address { get; set; }
        
        [Required]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "National number must be exactly 11 digits")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "National number must contain only digits")]
        public string NationalNumber { get; set; } = null!;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
        
        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = null!;
        
        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = null!;
        
        public string? PhoneNumber { get; set; }
    }
    public class AdminResetPasswordDto
    {
        public string Email { get; set; }
        public string NewPassword { get; set; }
    }

    // Permission Management DTOs
    public class PermissionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }

    public class CreatePermissionDto
    {
        [Required]
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }

    public class UpdatePermissionDto
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }

    // Role Management DTOs
    public class RoleDto
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public List<PermissionDto> Permissions { get; set; } = new();
    }

    public class CreateRoleDto
    {
        [Required]
        public string Name { get; set; } = null!;
        public List<int> PermissionIds { get; set; } = new();
    }

    public class UpdateRoleDto
    {
        [Required]
        public string Id { get; set; } = null!;
        [Required]
        public string Name { get; set; } = null!;
        public List<int> PermissionIds { get; set; } = new();
    }

    public class AssignPermissionsToRoleDto
    {
        [Required]
        public string RoleId { get; set; } = null!;
        [Required]
        public List<int> PermissionIds { get; set; } = new();
    }

    public class UserRoleDto
    {
        public string UserId { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public List<string> Roles { get; set; } = new();
        public List<string> Permissions { get; set; } = new();
    }
}
