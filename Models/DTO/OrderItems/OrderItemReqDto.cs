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
        public string PrintName { get; set; } = string.Empty;

        [Required]
        public string ItemUuid { get; set; }

        [Required]
        [Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public decimal Quantity { get; set; }

        /// <summary>
        /// Optional discount ratio for this specific item
        /// </summary>
        [Range(0, 100)]
        public decimal DiscountRatio { get; set; }

        /// <summary>
        /// Marked price provided by frontend. Service will store this value
        /// on the order item as the item's marked price at sale.
        /// </summary>
        [Required]
        [Range(0, double.MaxValue)]
        public decimal MarkedPrice { get; set; }

        /// <summary>
        /// Sale price (unit price after discount) provided by frontend.
        /// Service trusts this value for LineTotal calculation.
        /// </summary>
        [Required]
        [Range(0, double.MaxValue)]
        public decimal SalePrice { get; set; }

        /// <summary>
        /// Line total provided by frontend. Service will accept this value
        /// and will not recalculate it from master data.
        /// </summary>
        [Required]
        [Range(0, double.MaxValue)]
        public decimal LineTotal { get; set; }
    }
}
