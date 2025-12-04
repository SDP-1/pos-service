using pos_service.Models.Enums;

namespace pos_service.Models
{
    public class RolePermission
    {
        public int Id { get; set; }

        public UserRole Role { get; set; }

        public int PermissionId { get; set; }
        public Permission Permission { get; set; } = null!;

        public string Uuid { get; set; } = string.Empty;
    }
}