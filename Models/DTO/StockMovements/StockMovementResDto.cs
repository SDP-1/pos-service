using pos_service.Models.Enums;

namespace pos_service.Models.DTO.StockMovements
{
    /// <summary>
    /// Response Data Transfer Object representing an inventory stock movement ledger entry.
    /// Details stock transitions (IN, OUT, ADJUSTMENT), movement reasons, quantities, cost price, and audit metadata.
    /// </summary>
    public class StockMovementResDto
    {
        /// <summary>
        /// The unique primary key identifier of the stock movement ledger entry.
        /// </summary>
        public long Id                          { get; set; }

        /// <summary>
        /// Globally unique identifier (UUID) for this movement.
        /// </summary>
        public string Uuid                      { get; set; } = string.Empty;

        /// <summary>
        /// The UUID of the associated inventory batch.
        /// </summary>
        public string BatchUuid                 { get; set; } = string.Empty;

        /// <summary>
        /// The UUID of the associated item.
        /// </summary>
        public string ItemUuid                  { get; set; } = string.Empty;

        /// <summary>
        /// Display name of the item.
        /// </summary>
        public string? ItemName                 { get; set; }

        /// <summary>
        /// Batch number of the associated batch.
        /// </summary>
        public string? BatchNumber              { get; set; }

        /// <summary>
        /// The trigger type of movement (Purchase, Sale, Return, Damage, Adjustment, Transfer, Count).
        /// </summary>
        public StockMovementType MovementType   { get; set; }

        /// <summary>
        /// Quantity of stock moved.
        /// </summary>
        public decimal Quantity                 { get; set; }

        /// <summary>
        /// Direction of movement (IN = stock added, OUT = stock removed).
        /// </summary>
        public StockMovementDirection Direction { get; set; }

        /// <summary>
        /// Cost price of the item at the time of movement.
        /// </summary>
        public decimal CostPrice                { get; set; }

        /// <summary>
        /// Source entity type that caused the movement (e.g. Purchase, Order, OrderReturn, OrderDelete, ManualBatch).
        /// </summary>
        public StockMovementReferenceType? ReferenceType { get; set; }

        /// <summary>
        /// Reference UUID of the source transaction.
        /// </summary>
        public string? ReferenceUuid            { get; set; }

        /// <summary>
        /// Reason or category description for the movement.
        /// </summary>
        public string? Reason                   { get; set; }

        /// <summary>
        /// Additional notes or user comments.
        /// </summary>
        public string? Comment                  { get; set; }

        /// <summary>
        /// Timestamp when the movement occurred.
        /// </summary>
        public DateTime CreatedAt               { get; set; }

        /// <summary>
        /// UUID of the user who performed the movement.
        /// </summary>
        public string? CreatedBy                { get; set; }

        /// <summary>
        /// Full name of the user who performed the movement.
        /// </summary>
        public string? CreatedByName            { get; set; }
    }
}
