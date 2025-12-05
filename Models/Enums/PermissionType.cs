namespace pos_service.Models.Enums
{
    // Permission enum values MUST match Permission.Id values in the database.
    public enum PermissionType
    {
        // Orders
        ORDER_VIEW = 1,
        ORDER_ADD = 2,
        ORDER_UPDATE = 3,
        ORDER_DELETE = 4,
        ORDER_DELETE_PERMANENTLY = 5,
        ORDER_UPDATE_STATUS = 6,

        // Items
        ITEM_VIEW = 10,
        ITEM_ADD = 11,
        ITEM_UPDATE = 12,
        ITEM_DELETE = 13,
        ITEM_ADD_STOCK = 14,

        // Users
        USER_VIEW = 20,
        USER_CREATE = 21,
        USER_UPDATE = 22,
        USER_DEACTIVATE = 23,
        USER_DELETE = 24,
        USER_CHANGE_PASSWORD = 25,
        USER_MANAGE = 26,

        // Suppliers
        SUPPLIER_VIEW = 30,
        SUPPLIER_CREATE = 31,
        SUPPLIER_UPDATE = 32,
        SUPPLIER_DELETE = 33,

        // Contacts
        CONTACT_VIEW = 40,
        CONTACT_CREATE = 41,
        CONTACT_UPDATE = 42,
        CONTACT_DELETE = 43,

        // Permissions & Roles
        PERMISSION_VIEW = 50,
        PERMISSION_ASSIGN = 51,
        ROLE_VIEW = 60,
        ROLE_CREATE = 61,
        ROLE_UPDATE = 62,
        ROLE_DELETE = 63
    }
}
