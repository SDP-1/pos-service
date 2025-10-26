namespace pos_service.Models.Enums
{
    public enum UserRole
    {
        // System-level role, can manage everything across all shops
        SystemAdmin = 1,

        // Manages users, settings, and reports for a specific shop
        ShopAdmin   = 2,

        // Manages daily operations, voids, returns, and supervises cashiers
        Manager     = 3,

        // Primary user of the POS terminal for transactions
        Cashier     = 4,

        // Manages inventory, receives stock, and handles suppliers
        StockKeeper = 5,

        // Can view financial reports and logs but cannot perform transactions
        Auditor     = 6
    }

    /// <summary>
    /// Provides constant string values for all UserRole enums, 
    /// ensuring the strings exactly match the enum member names.
    /// </summary>
    public static class UserRoles
    {
        // Role Constants derived directly from the Enum names
        public const string SystemAdmin         = nameof(UserRole.SystemAdmin);
        public const string ShopAdmin           = nameof(UserRole.ShopAdmin);
        public const string Manager             = nameof(UserRole.Manager);
        public const string Cashier             = nameof(UserRole.Cashier);
        public const string StockKeeper         = nameof(UserRole.StockKeeper);
        public const string Auditor             = nameof(UserRole.Auditor);

        // Helper constants for common role groups, also using nameof()
        public const string AllAdmins           = SystemAdmin + "," + ShopAdmin;
        public const string Management          = SystemAdmin + "," + ShopAdmin + "," + Manager;
        public const string InventoryManagement = Management + "," + StockKeeper;
        public const string DayToDayOperations  = Management + "," + Cashier;
        public const string AllUsers            = Management + "," + Cashier + "," + StockKeeper + "," + Auditor;
    }
}
