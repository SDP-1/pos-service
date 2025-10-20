namespace pos_service.Models.DTO
{
    public class ContactResDto
    {
        public int Id { get; set; }
        public Guid Uuid { get; set; }
        public string Name { get; set; }
        public string? Designation { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; }
        public int? UserId { get; set; }

        // 🔹 Relationship: (optional) - Foreign key for the Supplier
        public int? SupplierId { get; set; }
    }
}
