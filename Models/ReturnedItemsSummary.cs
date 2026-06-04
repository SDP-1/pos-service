using System;

namespace pos_service.Models
{
    // Keyless entity mapped to DB view 'view_returned_items_summary'
    public class ReturnedItemsSummary
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public string OrderUuid { get; set; }

        public string PrintName { get; set; }
        public string ReturnedOrderItemUuid { get; set; }

        public decimal OriginalPurchasedQty { get; set; }
        public decimal TotalReturnedQty { get; set; }
        public decimal RemainingQty { get; set; }

        public decimal PriceAtSale { get; set; }
        public decimal TotalRefundAmountValue { get; set; }
    }
}
