using pos_service.Models.DTO.OrderItems;
using pos_service.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace pos_service.Models.DTO.Orders
{
    public class OrderReqDto
    {
        [Required]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

        [Required]
        public SaleType SaleType { get; set; } = SaleType.Retail;

        /// <summary>
        /// The amount of money received from the customer.
        /// </summary>
        [Required]
        [Range(double.MinValue, double.MaxValue)]
        public decimal AmountPaid { get; set; }

        /// <summary>
        /// Optional gross amount calculated by frontend. When provided, service will
        /// use this instead of recalculating from item master data.
        /// </summary>
        [Required]
        [Range(double.MinValue, double.MaxValue)]
        public decimal GrossAmount { get; set; }

        /// <summary>
        /// Total discount calculated by frontend.
        /// </summary>
        [Required]
        [Range(double.MinValue, double.MaxValue)]
        public decimal TotalDiscount { get; set; }

        /// <summary>
        /// Net amount (GrossAmount - TotalDiscount) provided by frontend.
        /// </summary>
        [Required]
        [Range(double.MinValue, double.MaxValue)]
        public decimal NetAmount { get; set; }

        /// <summary>
        /// Number of unique items in the order as calculated by the frontend.
        /// Required so the service does not have to recalculate it.
        /// </summary>
        [Required]
        [Range(0, int.MaxValue)]
        public int ItemCount { get; set; }

        /// <summary>
        /// Optional customer ID for registered customers
        /// </summary>
        public int? CustomerId { get; set; }

        /// <summary>
        /// Optional description or notes for the order
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Collection of order items
        /// </summary>
        [Required]
        [MinLength(1, ErrorMessage = "At least one order item is required")]
        public List<OrderItemReqDto> OrderItems { get; set; } = new List<OrderItemReqDto>();
    }
}
