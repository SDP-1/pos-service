namespace pos_service.Models.Enums
{
    public enum UserRole
    {
        // System-level role, can manage everything across all shops
        SystemAdmin,

        // Manages users, settings, and reports for a specific shop
        ShopAdmin,

        // Manages daily operations, voids, returns, and supervises cashiers
        Manager,

        // Primary user of the POS terminal for transactions
        Cashier,

        // Manages inventory, receives stock, and handles suppliers
        StockKeeper,

        // Can view financial reports and logs but cannot perform transactions
        Auditor
    }
}
