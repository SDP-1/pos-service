using pos_service.Models.DTO.Audits;
using pos_service.Models.DTO.Contacts;

namespace pos_service.Models.DTO.Suppliers
{
    public class SupplierResDto : IFullResAuditDto
    {
        public int Id                       { get; set; }
        public string Name                  { get; set; }
        public string? Address              { get; set; }
        public List<ContactResDto> contacts { get; set; }

        public string Uuid                    { get; set; }
        public DateTime CreatedAt           { get; set; }
        public DateTime? UpdatedAt          { get; set; }
        public string CreatedBy             { get; set; }
        public string? UpdatedBy            { get; set; }
        public bool IsActive                { get; set; }
    }
}
