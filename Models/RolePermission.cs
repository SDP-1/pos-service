using System.ComponentModel.DataAnnotations.Schema;
using pos_service.Models.Enums;

namespace pos_service.Models
{
    public class RolePermission
    {
        public int Id { get; set; }

        // replaced enum with FK to Role entity
        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;

        public int PermissionId { get; set; }
        public Permission Permission { get; set; } = null!;

        public string Uuid { get; set; } = string.Empty;
    }
}