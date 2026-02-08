using pos_service.Models.DTO.Audits;
using System.ComponentModel.DataAnnotations;

namespace pos_service.Models.DTO.Contacts
{
    public class ContactReqDto : IReqAuditDto
    {
        /// <summary>
        ///  The unique identifier for the entity.
        ///  This help for when editing existing records.
        ///  This is special case refer SupplierService UpdateSupplierAsync method for more understanding
        /// </summary>
        public string? Uuid        { get; set; }

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
