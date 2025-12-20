using pos_service.Models.Audit;
using pos_service.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pos_service.Models
{
    public class Order : IAuditable
    {
        public int Id                                    { get; set; }

        /// <summary>
        /// A user-friendly, unique identifier for the order (e.g., ORD-2025-00001).
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string OrderNumber                        { get; set; }

        public OrderStatus Status                        { get; set; } = OrderStatus.Pending;
        public PaymentMethod PaymentMethod               { get; set; } = PaymentMethod.Cash;

        /// <summary>
        /// True if it's a retail sale, false if it's a wholesale transaction.
        /// </summary>
        public SaleType SaleType                         { get; set; } = SaleType.Reatail;

        /// <summary>
        /// The total number of unique items in the order.
        /// </summary>
        public int ItemCount                             { get; set; }

        /// <summary>
        /// The sum of (Price * Quantity) for all items before any discounts.
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal GrossAmount                       { get; set; } = 0.0m;

        /// <summary>
        /// The total discount amount applied to the entire order.
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalDiscount                     { get; set; } = 0.0m;

        /// <summary>
        /// The final payable amount (GrossAmount - TotalDiscount).
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal NetAmount                         { get; set; } = 0.0m;

        /// <summary>
        /// The total cost of all goods sold in this order (sum of BuyingPrice).
        /// Used to calculate profit.
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalCost                         { get; set; } = 0.0m;

        /// <summary>
        /// The amount of money received from the customer.
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal AmountPaid                        { get; set; } = 0.0m;

        /// <summary>
        /// The balance to be paid or returned to the customer (change).
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Balance                           { get; set; } = 0.0m;

        // --- Foreign Key to User (Cashier) ---
        public int CashierId                             { get; set; }
        public virtual User Cashier                      { get; set; }

        // --- Optional Foreign Key to Customer ---
        /// <summary>
        /// The foreign key for the customer who placed the order.
        /// Can be null for anonymous walk-in sales.
        /// </summary>
        public int? CustomerId                           { get; set; }

        public virtual Customer Customer                 { get; set; }

        /// <summary>
        /// A collection of order irem associated with this order.
        /// </summary>
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        /// <summary>
        /// Optional description or notes for the order.
        /// Default is null.
        /// </summary>
        public string? Description                       { get; set; } = null;

        // --- Implementation of IAuditable ---
        public string Uuid                               { get; set; }
        public DateTime CreatedAt                        { get; set; }
        public DateTime? UpdatedAt                       { get; set; }
        public string CreatedBy                          { get; set; }
        public string? UpdatedBy                         { get; set; }
        public bool IsActive                             { get; set; } = true;
    }
}
