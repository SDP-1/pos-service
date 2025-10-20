using System.ComponentModel.DataAnnotations;

namespace pos_service.Models
{
    public class Supplier : IAuditable
    {
        /// <summary>
        /// The integer primary key for the database.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The official name of the supplier company.
        /// </summary>
        [Required]
        [MaxLength(150)]
        public string Name { get; set; }

        /// <summary>
        /// The physical or mailing address of the supplier.
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// A collection of items provided by this supplier.
        /// </summary>
        public virtual ICollection<Item> Items { get; set; } = new List<Item>();

        /// <summary>
        /// A collection of contacts associated with this supplier.
        /// </summary>
        public virtual ICollection<Contact> Contacts { get; set; } = new List<Contact>();

        // --- Implementation of IAuditable ---
        public Guid Uuid { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
