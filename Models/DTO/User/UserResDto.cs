using pos_service.Models.DTO.Audit;
using pos_service.Models.DTO.Contact;
using pos_service.Models.Enums;

namespace pos_service.Models.DTO.User
{
    public class UserResDto : IReqAuditDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; } // Email is exposed for display
        public UserRole Role { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? NIC { get; set; }
        public bool IsActive { get; set; }

        // Include related DTOs if necessary
        public ICollection<ContactResDto> Contacts { get; set; } = new List<ContactResDto>();
    }
}
