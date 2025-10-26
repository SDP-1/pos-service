using pos_service.Models.DTO.Audit;
using System.ComponentModel.DataAnnotations;

namespace pos_service.Models.DTO.Contact
{
    public class ContactReqDto : IReqAuditDto
    {
        [Required]
        [MaxLength(100)]
        public string Name         { get; set; }

        [MaxLength(100)]
        public string? Designation { get; set; }

        [Phone]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [EmailAddress]
        [MaxLength(100)]
        public string? Email       { get; set; }

        public int? UserId         { get; set; }
        public int? SupplierId     { get; set; }

        public bool IsActive       { get; set; } = true;
    }

}
