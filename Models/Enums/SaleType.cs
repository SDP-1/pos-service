namespace pos_service.Models.Enums
{
    /// <summary>
    /// Type of sale for pricing and reporting purposes.
    /// </summary>
    public enum SaleType
    {
        /// <summary>
        /// Default / unspecified sale type.
        /// </summary>
        Default    = 0,

        /// <summary>
        /// Retail sale to end customers.
        /// </summary>
        Retail     = 1,

        /// <summary>
        /// Wholesale sale to bulk buyers.
        /// </summary>
        Wholesale  = 2
    }
}
