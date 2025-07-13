namespace Domain.IdentityEntiities
{
    public class Permission
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public ICollection<PermissionRole> PermissionRoles { get; set; } = new List<PermissionRole>();
    }
}
