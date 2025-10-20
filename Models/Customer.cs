using System.ComponentModel.DataAnnotations;

namespace pos_service.Models
{
    public class Customer : IAuditable
    {
        /// <summary>
        /// The unique primary key for the customer.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The customer's first name.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        /// <summary>
        /// The customer's last name.
        /// </summary>
        [MaxLength(50)]
        public string? LastName { get; set; }

        /// <summary>
        /// The customer's primary contact number, used for identification.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// The customer's email address. Optional.
        /// </summary>
        [EmailAddress]
        [MaxLength(100)]
        public string? Email { get; set; }

        /// <summary>
        /// The customer's physical address. Optional.
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// The total loyalty points accumulated by the customer.
        /// </summary>
        public int LoyaltyPoints { get; set; } = 0;

        // --- Implementation of IAuditable ---
        public Guid Uuid { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
