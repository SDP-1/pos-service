using pos_service.Models.DTO.Audits;
using System.ComponentModel.DataAnnotations;

namespace pos_service.Models.DTO.Suppliers
{
    public class SupplierReqDto : IReqAuditDto
    {
        [Required]
        [MaxLength(150)]
        public string Name     { get; set; }

        public string? Address { get; set; }

        public bool IsActive   { get; set; } = true;
    }
}
