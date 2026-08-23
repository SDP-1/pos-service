using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pos_service.Models
{
    /// <summary>
    /// Entity representing an immutable log entry in <c>tbl_order_logs</c>.
    /// Automatically captured via database triggers on order INSERT, UPDATE, and DELETE actions.
    /// </summary>
    public class OrderLog
    {
        [Key]
        [Column("LogId")]
        public long Id { get; set; }

        public int OrderId { get; set; }

        [Required]
        [MaxLength(50)]
        public string OrderNumber { get; set; }

        [Required]
        [MaxLength(50)]
        public string MainStatus { get; set; }

        [MaxLength(50)]
        public string? SubStatus { get; set; }

        [Required]
        [MaxLength(50)]
        public string PaymentMethod { get; set; }

        [Required]
        [MaxLength(50)]
        public string SaleType { get; set; }

        public int ItemCount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal GrossAmount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalDiscount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal NetAmount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalCost { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal AmountPaid { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Balance { get; set; }

        public int? CashierId { get; set; }
        public int? CustomerId { get; set; }

        public string? Description { get; set; }

        [Required]
        [MaxLength(36)]
        public string OrderUuid { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [MaxLength(36)]
        public string? CreatedBy { get; set; }

        [MaxLength(36)]
        public string? UpdatedBy { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        [MaxLength(10)]
        public string Action { get; set; }

        public DateTime ActionDate { get; set; }

        [MaxLength(36)]
        public string? ActionBy { get; set; }

        public virtual User? ActionByUser { get; set; }
    }
}
