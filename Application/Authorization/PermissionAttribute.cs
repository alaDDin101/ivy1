using Microsoft.AspNetCore.Authorization;

namespace Application.Authorization
{
    public class PermissionAttribute : AuthorizeAttribute
    {
        public PermissionAttribute(string permission) : base(permission)
        {
            Policy = permission;
        }
    }

    public static class Permissions
    {
        // Patient Management
        public const string ViewPatients = "view_patients";
        public const string CreatePatients = "create_patients";
        public const string UpdatePatients = "update_patients";
        public const string DeletePatients = "delete_patients";

        // Doctor Management
        public const string ViewDoctors = "view_doctors";
        public const string CreateDoctors = "create_doctors";
        public const string UpdateDoctors = "update_doctors";
        public const string DeleteDoctors = "delete_doctors";

        // Clinic Management
        public const string ViewClinics = "view_clinics";
        public const string CreateClinics = "create_clinics";
        public const string UpdateClinics = "update_clinics";
        public const string DeleteClinics = "delete_clinics";

        // Appointment Management
        public const string ViewAppointments = "view_appointments";
        public const string CreateAppointments = "create_appointments";
        public const string UpdateAppointments = "update_appointments";
        public const string DeleteAppointments = "delete_appointments";
        public const string BookAppointments = "book_appointments";
        public const string ConfirmAppointments = "confirm_appointments";

        // Role and Permission Management
        public const string ViewRoles = "view_roles";
        public const string CreateRoles = "create_roles";
        public const string UpdateRoles = "update_roles";
        public const string DeleteRoles = "delete_roles";
        public const string ViewPermissions = "view_permissions";
        public const string CreatePermissions = "create_permissions";
        public const string UpdatePermissions = "update_permissions";
        public const string DeletePermissions = "delete_permissions";
        public const string ManageRolePermissions = "manage_role_permissions";

        // System Administration
        public const string ViewSystemSettings = "view_system_settings";
        public const string UpdateSystemSettings = "update_system_settings";
        public const string ViewAuditLogs = "view_audit_logs";
        public const string ManageUsers = "manage_users";

        // Get all permissions for seeding
        public static IEnumerable<string> GetAllPermissions()
        {
            return typeof(Permissions)
                .GetFields()
                .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
                .Select(f => (string)f.GetValue(null)!)
                .Where(v => !string.IsNullOrEmpty(v));
        }
    }
} 