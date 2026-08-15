using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using pos_service.Models.Audit;

namespace pos_service.Models
{
    public class Role : IAuditable
    {
        public int Id                              { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name                         { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? Description                 { get; set; }

        // Hierarchy link
        public int? ParentRoleId                   { get; set; }

        [JsonIgnore]
        public Role? ParentRole                    { get; set; }

        [JsonIgnore]
        public ICollection<Role> ChildRoles        { get; set; } = new List<Role>();

        // Direct permissions assigned to this role
        public ICollection<Permission> Permissions { get; set; } = new List<Permission>();

        // --- Implementation of IAuditable ---
        public string Uuid         { get; set; }
        public DateTime CreatedAt  { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy    { get; set; }
        public string? UpdatedBy   { get; set; }
        public bool IsActive       { get; set; } = true;
    }
}
