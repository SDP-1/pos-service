namespace pos_service.Models.Enums
{
    /// <summary>
    /// Indicates whether a stock movement transaction increases (IN) or decreases (OUT) inventory balance.
    /// </summary>
    public enum StockMovementDirection
    {
        /// <summary>
        /// Stock is added / increased in inventory (e.g., purchase receipts, customer return restocking, positive adjustment).
        /// </summary>
        IN  = 1,

        /// <summary>
        /// Stock is removed / decreased from inventory (e.g., sales orders, supplier returns, waste/damage write-offs, negative adjustment).
        /// </summary>
        OUT = 2
    }
}
