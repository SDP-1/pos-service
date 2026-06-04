using pos_service.Models.Audit;
using System.ComponentModel.DataAnnotations;

namespace pos_service.Models
{
    public class Shop : IAuditable
    {
        /// <summary>
        /// Integer primary key.
        /// </summary>
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(1000)]
        public string? Address { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(255)]
        public string? Email { get; set; }

        /// <summary>
        /// Binary contents of the shop logo. Stored in DB as BLOB/MEDIUMBLOB/longblob.
        /// Use byte[] so EF Core maps this to a binary column.
        /// </summary>
        public byte[]? Logo { get; set; }

        // --- IAuditable ---
        public string Uuid { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
