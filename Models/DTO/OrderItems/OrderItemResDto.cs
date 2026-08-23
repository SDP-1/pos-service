using pos_service.Models.DTO.Audits;
using pos_service.Models.DTO.ReturnedItems;

namespace pos_service.Models.DTO.OrderItems
{
    /// <summary>
    /// Response Data Transfer Object representing an individual line item in an order.
    /// Captures snapshotted prices at time of sale (sale price, marked price, unit cost), quantity, line total, return flags, and batch linkage.
    /// </summary>
    public class OrderItemResDto : IFullResAuditDto
    {
        /// <summary>
        /// The unique primary key identifier of the order line item.
        /// </summary>
        public int Id                                    { get; set; }

        /// <summary>
        /// The identifier of the parent sales order.
        /// </summary>
        public int OrderId                               { get; set; }

        /// <summary>
        /// The UUID of the catalog item at the time of sale.
        /// </summary>
        public string? OriginalItemUuid                  { get; set; }

        /// <summary>
        /// The UUID of the allocated inventory batch.
        /// </summary>
        public string? BatchUuid                         { get; set; }

        /// <summary>
        /// The snapshotted receipt print name for this item.
        /// </summary>
        public string PrintName                          { get; set; }

        /// <summary>
        /// The quantity of items sold.
        /// </summary>
        public decimal Quantity                          { get; set; }

        /// <summary>
        /// The unit selling price charged at the moment of sale.
        /// </summary>
        public decimal PriceAtSale                       { get; set; }

        /// <summary>
        /// The manufacturer marked price (MRP) at the moment of sale.
        /// </summary>
        public decimal MarkedPriceAtSale                 { get; set; }

        /// <summary>
        /// The unit cost of the item at the moment of sale.
        /// </summary>
        public decimal CostAtSale                        { get; set; }

        /// <summary>
        /// The final calculated line total (Quantity * PriceAtSale minus line discount).
        /// </summary>
        public decimal LineTotal                         { get; set; }

        /// <summary>
        /// Indicates if this item supports fractional / decimal quantities.
        /// </summary>
        public bool AllowsDecimalQuantities              { get; set; }

        /// <summary>
        /// True if this line item represents a customer return / refund.
        /// </summary>
        public bool IsReturnItem                         { get; set; }

        /// <summary>
        /// Optional line item notes or return remarks.
        /// </summary>
        public string? Description                       { get; set; }

        /// <summary>
        /// UUID reference to the original order line being returned, if applicable.
        /// </summary>
        public string? ReturnedOrderItemUuid             { get; set; }

        // Return summary (populated when requesting order with returns)

        /// <summary>
        /// Summary details of return items associated with this line.
        /// </summary>
        public ReturnedItemsSummaryResDto? ReturnSummary { get; set; }

        // Audit fields

        /// <summary>
        /// Globally unique identifier (UUID) for this order item.
        /// </summary>
        public string Uuid                               { get; set; }

        /// <summary>
        /// Timestamp when the line item was created.
        /// </summary>
        public DateTime CreatedAt                        { get; set; }

        /// <summary>
        /// Timestamp when the line item was last updated.
        /// </summary>
        public DateTime? UpdatedAt                       { get; set; }

        /// <summary>
        /// Username of the user who recorded this line item.
        /// </summary>
        public string CreatedBy                          { get; set; }

        /// <summary>
        /// Username of the user who last updated this line item.
        /// </summary>
        public string? UpdatedBy                         { get; set; }

        /// <summary>
        /// Indicates whether this order item record is active.
        /// </summary>
        public bool IsActive                             { get; set; }
    }
}
