using pos_service.Models.DTO.Audit;
using pos_service.Models.DTO.Contact;

namespace pos_service.Models.DTO.Supplier
{
    public class SupplierResDto : IFullResAuditDto
    {
        public int Id                       { get; set; }
        public string Name                  { get; set; }
        public string? Address              { get; set; }
        public List<ContactResDto> contacts { get; set; }

        public Guid Uuid                    { get; set; }
        public DateTime CreatedAt           { get; set; }
        public DateTime? UpdatedAt          { get; set; }
        public string CreatedBy             { get; set; }
        public string? UpdatedBy            { get; set; }
        public bool IsActive                { get; set; }
    }
}
