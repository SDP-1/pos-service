using pos_service.Models.DTO.Contacts;
using System.ComponentModel.DataAnnotations;

namespace pos_service.Models.DTO.Users
{ 
    public class UserReqDto
    {
        [MaxLength(50)]
        public string? FirstName { get; set; }

        [MaxLength(50)]
        public string? LastName { get; set; }

        [EmailAddress]
        public string? UserName { get; set; }

        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string? Password { get; set; } // Note: This will be HASHED in the Service layer

        public int? RoleId { get; set; }

        // The client provides a URL to the profile image; may be null if no image.
        public string? ProfileImageUrl { get; set; }

        [MaxLength(12)]
        public string? NIC { get; set; }

        // Optional list of contacts to create/update for this user.
        // If null, service will not modify contacts.
        // Existing contacts are identified by uuid; new ones have null uuid.
        public ICollection<ContactReqDto>? Contacts { get; set; }
    }
}
