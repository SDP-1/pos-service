using pos_service.Models.Audit;
using pos_service.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pos_service.Models
{
    public class Inventory : IAuditable
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string ItemUuid { get; set; }

        [Column(TypeName = "decimal(18, 3)")]
        public decimal StockQuantity { get; set; } = 0;

        public bool AllowsDecimalQuantities { get; set; } = false;

        public UnitType UnitType { get; set; } = UnitType.Each;

        public virtual Item Item { get; set; }

        public virtual ICollection<InventoryUnit> Units { get; set; } = new List<InventoryUnit>();

        public string Uuid { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
