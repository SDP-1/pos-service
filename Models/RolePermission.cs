using System.ComponentModel.DataAnnotations.Schema;
using pos_service.Models.Audit;
using pos_service.Models.Enums;

namespace pos_service.Models
{
    public class RolePermission : IAuditable
    {
        public int Id                { get; set; }

        // replaced enum with FK to Role entity
        public int RoleId            { get; set; }
        public Role Role             { get; set; } = null!;

        public int PermissionId      { get; set; }
        public Permission Permission { get; set; } = null!;

        // --- Implementation of IAuditable ---
        public string Uuid           { get; set; }
        public DateTime CreatedAt    { get; set; }
        public DateTime? UpdatedAt   { get; set; }
        public string CreatedBy      { get; set; }
        public string? UpdatedBy     { get; set; }
        public bool IsActive         { get; set; } = true;
    }
}