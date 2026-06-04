using pos_service.Models.DTO.Audits;

namespace pos_service.Models.DTO.Customers
{
    public class CustomerResDto : IFullResAuditDto
    {
        public int Id              { get; set; }
        public string FirstName    { get; set; }
        public string? LastName    { get; set; }
        public string FullName     { get; set; }
        public string PhoneNumber  { get; set; }
        public string? Email       { get; set; }
        public string? Address     { get; set; }
        public int LoyaltyPoints   { get; set; }

        public string Uuid         { get; set; }
        public DateTime CreatedAt  { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy    { get; set; }
        public string? UpdatedBy   { get; set; }
        public bool IsActive       { get; set; }
    }
}
