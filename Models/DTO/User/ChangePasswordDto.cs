using System.ComponentModel.DataAnnotations;

namespace pos_service.Models.DTO.User
{
    public class ChangePasswordDto
    {
        /// <summary>
        /// The user's current password, required for verification.
        /// </summary>
        [Required]
        public string OldPassword { get; set; }

        /// <summary>
        /// The new password the user wishes to set.
        /// </summary>
        [Required]
        [MinLength(6, ErrorMessage = "New password must be at least 6 characters long.")]
        public string NewPassword { get; set; }
    }
}
