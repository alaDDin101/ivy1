using Microsoft.AspNetCore.Identity;

namespace Domain.IdentityEntiities
{
    public class PermissionRole
    {
        public int Id { get; set; }
        public int PermissionId { get; set; }
        public Permission Permission { get; set; } = null!;

        public string RoleId { get; set; } = string.Empty;
        public IdentityRole Role { get; set; } = null!;
    }
}
