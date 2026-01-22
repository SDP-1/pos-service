using pos_service.Models.Audit;
using System.ComponentModel.DataAnnotations.Schema;

namespace pos_service.Models
{
    public class ItemSupplier : IAuditable
    {
        // Composite FK properties (configured in AppDbContext)
        public int SuppliersId { get; set; }

        public int ItemsId { get; set; }
        public int ItemsSubId { get; set; }

        // Navigation properties
        public virtual Supplier Supplier { get; set; }
        public virtual Item Item { get; set; }

        // IAuditable implementation
        public string Uuid { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
