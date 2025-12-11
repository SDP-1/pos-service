using pos_service.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace pos_service.Models.DTO.OrderItems
{
    public class OrderItemReqDto
    {
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

        // Update-specific fields (optional for updates)
        public int? Id { get; set; }
        public bool? IsDeleted { get; set; }
    }
}
