namespace pos_service.Models.Enums
{
    /// <summary>
    /// Supported payment methods for orders and settlements.
    /// </summary>
    public enum PaymentMethod
    {
        /// <summary>
        /// Default / unspecified payment method.
        /// </summary>
        Default      = 0,

        /// <summary>
        /// Cash payment.
        /// </summary>
        Cash         = 1,

        /// <summary>
        /// Card payment (debit/credit).
        /// </summary>
        Card         = 2,

        /// <summary>
        /// Mobile wallet payment (e.g., mobile apps).
        /// </summary>
        MobileWallet = 3,

        /// <summary>
        /// Bank transfer payment.
        /// </summary>
        BankTransfer = 4,
    }
}
