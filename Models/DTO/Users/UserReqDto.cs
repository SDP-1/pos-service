using System.ComponentModel.DataAnnotations;

namespace pos_service.Models.DTO.Users
{ 
    public class UserReqDto
    {
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string UserName { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; set; } // Note: This will be HASHED in the Service layer

        [Required]
        public int RoleId { get; set; }

        // The client provides a URL to the profile image; may be null if no image.
        public string? ProfileImageUrl { get; set; }

        [MaxLength(12)]
        public string? NIC { get; set; }

    }
}
