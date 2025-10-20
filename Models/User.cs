using pos_service.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace pos_service.Models
{
    public class User : IAuditable
    {
        /// <summary>
        /// Primary key for the user.
        /// </summary>
        public int Id                  { get; set; }

        /// <summary>
        /// User's first name.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string FirstName        { get; set; }

        /// <summary>
        /// User's last name.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string LastName         { get; set; }

        /// <summary>
        /// User's unique username, used for login.
        /// </summary>
        [Required]
        [EmailAddress]
        public string UserName         { get; set; }

        /// <summary>
        /// The hashed version of the user's password.
        /// </summary>
        [Required]
        public string PasswordHash     { get; set; }

        /// <summary>
        /// The user's role, which determines their permissions.
        /// </summary>
        public UserRole Role           { get; set; }

        /// <summary>
        /// The URL or file path to the user's profile picture.
        /// </summary>
        public string? ProfileImageUrl { get; set; }

        /// <summary>
        // The user's Sri Lankan National Identity Card number.
        /// </summary>
        [MaxLength(12)]
        public string NIC              { get; set; }

        /// <summary>
        /// A collection of contacts associated with this user.
        /// </summary>
        public virtual ICollection<Contact> Contacts { get; set; } = new List<Contact>();

        // --- Implementation of IAuditable ---
        [Required]
        public Guid Uuid               { get; set; }
        public DateTime CreatedAt      { get; set; }
        public DateTime? UpdatedAt     { get; set; }
        public string CreatedBy        { get; set; }
        public string? UpdatedBy       { get; set; }
        public bool IsActive           { get; set; } = true;
    }
}
