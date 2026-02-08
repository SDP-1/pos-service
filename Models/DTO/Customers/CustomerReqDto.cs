using pos_service.Models.DTO.Audits;
using System.ComponentModel.DataAnnotations;

namespace pos_service.Models.DTO.Customers
{
    public class CustomerReqDto : IReqAuditDto
    {
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [MaxLength(50)]
        public string? LastName { get; set; }

        [Required]
        [Phone]
        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        [EmailAddress]
        [MaxLength(100)]
        public string? Email { get; set; }

        public string? Address { get; set; }

        public int LoyaltyPoints { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }
}
