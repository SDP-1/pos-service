using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pos_service.Models
{
    /// <summary>
    /// Entity representing an immutable log entry in <c>tbl_order_item_logs</c>.
    /// Automatically captured via database triggers on order item INSERT, UPDATE, and DELETE actions.
    /// </summary>
    public class OrderItemLog
    {
        [Key]
        [Column("LogId")]
        public long Id { get; set; }

        public int OrderItemId { get; set; }

        public int OrderId { get; set; }

        [MaxLength(36)]
        public string? OriginalItemUuid { get; set; }

        [MaxLength(36)]
        public string? BatchUuid { get; set; }

        [Required]
        [MaxLength(255)]
        public string PrintName { get; set; }

        [MaxLength(100)]
        public string? BarCode { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal PriceAtSale { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal CostAtSale { get; set; }

        [Column(TypeName = "decimal(18, 3)")]
        public decimal Quantity { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Discount { get; set; } = 0.0m;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal LineTotal { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalProfit { get; set; } = 0.0m;

        [MaxLength(50)]
        public string UnitType { get; set; } = "Each";

        public bool IsReturnItem { get; set; } = false;

        [MaxLength(36)]
        public string? ReturnedOrderItemUuid { get; set; }

        [Required]
        [MaxLength(36)]
        public string OrderItemUuid { get; set; }

        [Required]
        [MaxLength(10)]
        public string Action { get; set; }

        public DateTime ActionDate { get; set; }

        [MaxLength(36)]
        public string? ActionBy { get; set; }

        public virtual User? ActionByUser { get; set; }
    }
}
