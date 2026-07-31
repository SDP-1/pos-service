using System.ComponentModel.DataAnnotations;

namespace pos_service.Models.DTO.Users
{
    /// <summary>
    /// Data Transfer Object for resetting a user's password.
    /// </summary>
    public class ResetPasswordDto
    {
        /// <summary>
        /// The new password to set for the user.
        /// </summary>
        [Required]
        [MinLength(6, ErrorMessage = "New password must be at least 6 characters long.")]
        public string NewPassword { get; set; }
    }
}
