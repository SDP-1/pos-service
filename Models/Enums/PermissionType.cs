namespace pos_service.Models.Enums
{
    // PermissionCatagory enum values start from 1
    public enum PermissionCatagory
    {
        ORDER = 1,
        ITEM = 2,
        USER = 3,
        SUPPLIER = 4,
        CONTACT = 5,
        PERMISSION = 6,
        ROLE = 7
    }

    // PermissionType enum values MUST match Permission.Id values in the database.
    // Start values at 100 as requested.
    public enum PermissionType
    {
        // Orders (start at 100)
        ORDER_VIEW = 100,
        ORDER_ADD,
        ORDER_UPDATE,
        ORDER_DELETE,
        ORDER_DELETE_PERMANENTLY,
        ORDER_UPDATE_STATUS,

        // Items (start at 110)
        ITEM_VIEW = 110,
        ITEM_ADD,
        ITEM_UPDATE,
        ITEM_DELETE,
        ITEM_ADD_STOCK,

        // Users (start at 120)
        USER_VIEW = 120,
        USER_CREATE,
        USER_UPDATE,
        USER_DEACTIVATE,
        USER_DELETE,
        USER_CHANGE_PASSWORD,
        USER_MANAGE,

        // Suppliers (start at 130)
        SUPPLIER_VIEW = 130,
        SUPPLIER_CREATE,
        SUPPLIER_UPDATE,
        SUPPLIER_DELETE,

        // Contacts (start at 140)
        CONTACT_VIEW = 140,
        CONTACT_CREATE,
        CONTACT_UPDATE,
        CONTACT_DELETE,

        // Permissions & Roles (start at 150)
        PERMISSION_VIEW = 150,
        PERMISSION_ASSIGN,
        ROLE_VIEW = 160,
        ROLE_CREATE,
        ROLE_UPDATE,
        ROLE_DELETE
    }
}
