using pos_service.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace pos_service.Models.DTO.OrderItems
{
    public class OrderItemReqDto
    {
        /// <summary>
        /// Printable name for the item. This value will be used on receipts
        /// </summary>
        [Required]
        public string PrintName              { get; set; } = string.Empty;

        [Required]
        public string ItemUuid               { get; set; }

        /// <summary>
        /// Optional Batch UUID specified by cashier (manual override).
        /// If omitted/empty, the backend FEFO allocation selects the earliest-expiry active batch.
        /// </summary>
        public string? BatchUuid             { get; set; }

        [Required]
        [Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public decimal Quantity              { get; set; }

        /// <summary>
        /// Marked price provided by frontend. Service will store this value
        /// on the order item as the item's marked price at sale.
        /// </summary>
        [Required]
        [Range(0, double.MaxValue)]
        public decimal MarkedPrice           { get; set; }

        /// <summary>
        /// Sale price (unit price after discount) provided by frontend.
        /// Service trusts this value for LineTotal calculation.
        /// </summary>
        [Required]
        [Range(0, double.MaxValue)]
        public decimal SalePrice             { get; set; }

        /// <summary>
        /// Line total provided by frontend. Service will accept this value
        /// and will not recalculate it from master data.
        /// </summary>
        [Required]
        [Range(double.MinValue, double.MaxValue)]
        public decimal LineTotal             { get; set; }

        /// <summary>
        /// Indicates if this item is a return/refund.
        /// When true, the quantity will be added back to the original item instead of being deducted.
        /// </summary>
        public bool IsReturnItem             { get; set; } = false;

        /// <summary>
        /// Optional description for this item line.
        /// Can be used for any item to provide additional context or notes.
        /// </summary>
        [MaxLength(500)]
        public string? Description           { get; set; }

        /// <summary>
        /// The UUID of the returned OrderItem (reference to the original line being returned).
        /// Required when IsReturnItem is true to track which specific OrderItem is being returned.
        /// </summary>
        [MaxLength(36)]
        public string? ReturnedOrderItemUuid { get; set; }
    }
}
