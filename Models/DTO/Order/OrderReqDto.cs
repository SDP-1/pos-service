using pos_service.Models.DTO.OrderItem;
using pos_service.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace pos_service.Models.DTO.Order
{
    public class OrderReqDto
    {
        [Required]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

        [Required]
        public SaleType SaleType { get; set; } = SaleType.Reatail;

        /// <summary>
        /// The amount of money received from the customer.
        /// </summary>
        [Required]
        [Range(0, double.MaxValue)]
        public decimal AmountPaid { get; set; }

        /// <summary>
        /// Optional customer ID for registered customers
        /// </summary>
        public int? CustomerId { get; set; }

        /// <summary>
        /// Collection of order items
        /// </summary>
        [Required]
        [MinLength(1, ErrorMessage = "At least one order item is required")]
        public List<OrderItemReqDto> OrderItems { get; set; } = new List<OrderItemReqDto>();
    }
}
