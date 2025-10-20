using System.ComponentModel.DataAnnotations;

namespace pos_service.Models
{
    public class Contact : IAuditable
    {
        /// <summary>
        /// The integer primary key for the database.
        /// </summary>
        public int Id              { get; set; }

        /// <summary>
        /// The full name of the contact person.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name         { get; set; }

        /// <summary>
        /// The contact's job title or role.
        /// </summary>
        [MaxLength(100)]
        public string? Designation { get; set; }

        /// <summary>
        /// The contact's phone number.
        /// </summary>
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// The contact's email address.
        /// </summary>
        [EmailAddress]
        [MaxLength(100)]
        public string? Email       { get; set; }

        // 🔹 Relationship: (optional) - Foreign key for the User
        public virtual int? UserId { get; set; }

        // 🔹 Relationship: (optional) - Foreign key for the Supplier
        public virtual int? SupplierId { get; set; }

        // --- Implementation of IAuditable ---
        [Required]
        public Guid Uuid           { get; set; }
        public DateTime CreatedAt  { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy    { get; set; }
        public string? UpdatedBy   { get; set; }
        public bool IsActive       { get; set; } = true;
    }
}
