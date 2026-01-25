using pos_service.Models.DTO.Audits;
using pos_service.Models.DTO.Contacts;
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

        // Optional list of item UUIDs to associate with this supplier.
        // If null, service will not modify item associations.
        public ICollection<string>? ItemUuids { get; set; }

        // Optional list of contacts to create/update for this supplier.
        // If null, service will not modify contacts.
        public ICollection<ContactReqDto>? Contacts { get; set; }
    }
}
