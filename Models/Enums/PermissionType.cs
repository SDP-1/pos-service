namespace pos_service.Models.Enums
{

    /// <summary>
    /// Represents built-in user roles. Note: roles may be populated/managed via initialization code.
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// Default role / unspecified.
        /// </summary>
        DEFAULT = 0,

        /// <summary>
        /// System administrator role with full privileges.
        /// </summary>
        SYSTEM_ADMIN = 1,
    }

    /// <summary>
    /// Categories for grouping permissions. Values start from 1.
    /// </summary>
    public enum PermissionCatagory
    {
        /// <summary>
        /// Default / unspecified category.
        /// </summary>
        DEFAULT    = 0,

        /// <summary>
        /// Order related permissions.
        /// </summary>
        ORDER      = 1,
        /// <summary>
        /// Item related permissions.
        /// </summary>
        ITEM       = 2,
        /// <summary>
        /// User related permissions.
        /// </summary>
        USER       = 3,
        /// <summary>
        /// Supplier related permissions.
        /// </summary>
        SUPPLIER   = 4,
        /// <summary>
        /// Contact related permissions.
        /// </summary>
        CONTACT    = 5,
        /// <summary>
        /// Permission management related permissions.
        /// </summary>
        PERMISSION = 6,
        /// <summary>
        /// Role management related permissions.
        /// </summary>
        ROLE       = 7,
        /// <summary>
        /// Setting management permissions.
        /// </summary>
        SETTING    = 8,
        /// <summary>
        /// Shop management permissions.
        /// </summary>
        SHOP       = 9,
        /// <summary>
        /// Customer related permissions.
        /// </summary>
        CUSTOMER   = 10,
        /// <summary>
        /// Report template related permissions.
        /// </summary>
        REPORT_TEMPLATE = 11,
        /// <summary>
        /// SQL template related permissions.
        /// </summary>
        SQL_TEMPLATE    = 12,
        /// <summary>
        /// Settings option related permissions.
        /// </summary>
        SETTING_OPTIONS = 13,
        /// <summary>
        /// Nav bar options related permissions.
        /// </summary>
        NAV_BAR         = 14,
    }

    /// <summary>
    /// Concrete permission ids used in the application. These values MUST match Permission.Id values in the database.
    /// Grouped by category with starting offsets (100, 200, ...).
    /// </summary>
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

        // Customer (start at 550)
        CUSTOMER_VIEW                       = 550,
        CUSTOMER_CREATE                     = 551,
        CUSTOMER_UPDATE                     = 552,
        CUSTOMER_DELETE                     = 553,

        // Permissions (start at 600)
        PERMISSION_VIEW = 600,
        PERMISSION_ASSIGN                   = 601,

        // Roles (start at 650)
        ROLE_VIEW                           = 650,
        ROLE_CREATE                         = 651,
        ROLE_UPDATE                         = 652,
        ROLE_DELETE                         = 653,

        // Settings (start at 700)
        SETTING_MANAGE                      = 700,

        // Shop details (start at 750)
        SHOP_DETAILS_UPDATE                 = 750,

        // Report Templates (start at 800)
        REPORT_TEMPLATE_CREATE              = 800,
        REPORT_TEMPLATE_VIEW                = 801,
        REPORT_TEMPLATE_EDIT                = 802,
        REPORT_TEMPLATE_DELETE              = 803,
        REPORT_TEMPLATE_DOWNLOAD            = 804,
        REPORT_TEMPLATE_ASSIGN              = 805,

        // SQL Templates (start at 850)
        SQL_TEMPLATE_VIEW                   = 850,
        SQL_TEMPLATE_CREATE                 = 851,
        SQL_TEMPLATE_EDIT                   = 852,
        SQL_TEMPLATE_DELETE                 = 853,

        // Setting Options (start at 900)
        SETTING_OPTIONS_BACKUP_VIEW         = 900,
        SETTING_OPTIONS_SHOP_VIEW           = 901,
        SETTING_OPTIONS_ROLES_VIEW          = 902,
        SETTING_OPTIONS_REPORTS_VIEW        = 903,
        SETTING_OPTIONS_SQL_TEMPLATES_VIEW  = 904,
        SETTING_OPTIONS_SYSTEM_VIEW         = 905,
        SETTING_OPTIONS_USERS_VIEW          = 906,

        // Only for SYS Admin (start at 1000 - 1050)
        // Special permission to allow viewing existence/details of the SystemAdmin role (id=1)
        PERMISSION_SYSADMIN_VIEW            = 1000,

        // Navigation Bar Options (start at 1100)
        NAV_BAR_HOME_VIEW                   = 1100,
        NAV_BAR_ORDERS_VIEW                 = 1101,
        NAV_BAR_ITEMS_VIEW                  = 1102,
        NAV_BAR_SUPPLIERS_VIEW              = 1103,
        NAV_BAR_CUSTOMERS_VIEW              = 1104,
        NAV_BAR_INVENTORY_VIEW              = 1105,
        NAV_BAR_REPORTS_VIEW                = 1106,
        NAV_BAR_SETTINGS_VIEW               = 1107,
        NAV_BAR_HELP_VIEW                   = 1108,
    }
}
