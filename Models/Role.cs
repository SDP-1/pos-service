using System.ComponentModel.DataAnnotations;

namespace pos_service.Models
{
    public class Role
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? Description { get; set; }

        public string Uuid { get; set; } = string.Empty;

        //public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
