using pos_service.Models.DTO.Audit;

namespace pos_service.Models.DTO.Contact
{
    public class ContactResDto : IFullResAuditDto
    {
        public int Id              { get; set; }
        public string Name         { get; set; }
        public string? Designation { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email       { get; set; }
        public int? UserId         { get; set; }

        // 🔹 Relationship: (optional) - Foreign key for the Supplier
        public int? SupplierId     { get; set; }


        public Guid Uuid { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsActive { get; set; }
    }
}
