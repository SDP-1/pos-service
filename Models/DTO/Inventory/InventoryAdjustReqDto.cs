using pos_service.Models.Enums;
using pos_service.Models.DTO.Items;
using System.ComponentModel.DataAnnotations;

namespace pos_service.Models.DTO.Inventory
{
    public class InventoryAdjustReqDto
    {
        [Required]
        public UnitType UnitType { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Quantity { get; set; }

        /// <summary>
        /// When true, quantity will be added; when false, subtracted.
        /// </summary>
        public bool Increase { get; set; } = true;

        /// <summary>
        /// Optional expiration dates to add/replace when adjusting inventory.
        /// If null -> do not modify expires. If empty list -> clear all expires.
        /// </summary>
        public List<ItemExpiryReqDto>? Expiries { get; set; }

        /// <summary>
        /// Optional price details to update for the related item when adjusting inventory.
        /// </summary>
        public ItemPriceReqDto? Price { get; set; }
    }
}
