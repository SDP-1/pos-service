using pos_service.Models.DTO.Contacts;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

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

        // The client provides the profile image as a file;
        public IFormFile? ProfileImage { get; set; }

        /// <summary>
        /// When true on update, instructs the server to remove the existing profile image (set to null).
        /// Default false. If a new ProfileImage file is provided it will replace the existing image;
        /// if RemoveImage is true the image will be removed regardless of file upload.
        /// </summary>
        public bool RemoveImage { get; set; } = false;

        [MaxLength(12)]
        public string? NIC { get; set; }

        // Optional list of contacts to create/update for this user.
        // If null, service will not modify contacts.
        // Existing contacts are identified by uuid; new ones have null uuid.
        public ICollection<ContactReqDto>? Contacts { get; set; }
    }
}
