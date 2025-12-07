using pos_service.Models.DTO.Audit;
using pos_service.Models.DTO.Contact;
using pos_service.Models;

namespace pos_service.Models.DTO.User
{
    public class UserResDto : IReqAuditDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        /// <summary>
        /// User's full name.
        /// </summary>
        public string FullName
        {
            get { return $"{FirstName} {LastName}"; }
        }

        public string UserName { get; set; } // Email is exposed for display

        // replaced enum role with RoleId and Role object
        public int RoleId { get; set; }
        public Role Role { get; set; }

        public string? ProfileImageUrl { get; set; }
        public string? NIC { get; set; }
        public bool IsActive { get; set; }

        // Include related DTOs if necessary
        public ICollection<ContactResDto> Contacts { get; set; } = new List<ContactResDto>();
    }
}
