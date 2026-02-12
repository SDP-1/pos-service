namespace pos_service.Models.Enums
{

    public enum UserRole  //This set automaticaly -> AddPermissionToRoleAsync
    {
        DEFAULT = 0,

        SYSTEM_ADMIN = 1,
    }

    // PermissionCatagory enum values start from 1
    public enum PermissionCatagory  //This set automaticaly -> AddPermissionToRoleAsync
    {
        DEFAULT    = 0,

        ORDER      = 1,
        ITEM       = 2,
        USER       = 3,
        SUPPLIER   = 4,
        CONTACT    = 5,
        PERMISSION = 6,
        ROLE       = 7,
        SETTING    = 8
    }

    // PermissionType enum values MUST match Permission.Id values in the database.
    // Start values at 100 as requested.
    public enum PermissionType
    {
        DEFAULT                             = 0,

        // Orders (start at 100)
        ORDER_VIEW                          = 100,
        ORDER_ADD                           = 101,
        ORDER_UPDATE                        = 102,
        ORDER_DELETE                        = 103,
        ORDER_DELETE_PERMANENTLY            = 104,
        ORDER_UPDATE_STATUS                 = 105,
        ORDER_SETTLEMENT                    = 106,

        // Items (start at 200)
        ITEM_VIEW                           = 200,
        ITEM_ADD                            = 201,
        ITEM_UPDATE                         = 202,
        ITEM_DELETE                         = 203,
        ITEM_ADD_STOCK                      = 204,

        // Users (start at 300)
        USER_VIEW                           = 300,
        USER_CREATE                         = 301,
        USER_UPDATE                         = 302,
        USER_ACTIVE_STATUS_CHANGE           = 303,
        USER_DELETE                         = 304,
        USER_CHANGE_PASSWORD                = 305,
        USER_MANAGE                         = 306,

        // Suppliers (start at 400)
        SUPPLIER_VIEW                       = 400,
        SUPPLIER_CREATE                     = 401,
        SUPPLIER_UPDATE                     = 402,
        SUPPLIER_DELETE                     = 403,

        // Contacts (start at 500)
        CONTACT_VIEW                        = 500,
        CONTACT_CREATE                      = 501,
        CONTACT_UPDATE                      = 502,
        CONTACT_DELETE                      = 503,

        // Permissions (start at 600)
        PERMISSION_VIEW                     = 600,
        PERMISSION_ASSIGN                   = 601,

        //Roles (start at 650)
        ROLE_VIEW                           = 650,
        ROLE_CREATE                         = 651,
        ROLE_UPDATE                         = 652,
        ROLE_DELETE                         = 653,

        // Settings (start at 700)
        SETTING_MANAGE                      = 700,

        // Only for SYS Admin (start at 1000 - 1050)
        // Special permission to allow viewing existence/details of the SystemAdmin role (id=1)
        PERMISSION_SYSADMIN_VIEW            = 1000,
    }
}
