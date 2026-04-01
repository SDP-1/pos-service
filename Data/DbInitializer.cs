using Microsoft.EntityFrameworkCore;
using pos_service.Models;
using pos_service.Models.Enums;
using pos_service.Security;

namespace pos_service.Data
{
    /// <summary>
    /// Database initializer for seeding the application with default data.
    /// Provides methods to populate the database with essential initial records.
    /// </summary>
    public class DbInitializer
    {
        /// <summary>
        /// Seeds the database with initial data including admin user, suppliers, and items.
        /// Applies pending migrations and ensures default records are present in the database.
        /// </summary>
        /// <param name="context">The application database context.</param>
        /// <param name="passwordHasher">The password hasher service for securing user passwords.</param>
        public static async Task SeedAsync(AppDbContext context, IPasswordHasher passwordHasher)
        {
            // Apply pending migrations
            //await context.Database.MigrateAsync();

            // Seed permissions (must be first so we can reference IDs in enum)
            await SeedPermissionsAsync(context);

            // Seed roles
            await SeedRolesAsync(context);

            // Seed role-permission mappings
            await SeedRolePermissionsAsync(context);

            // Seed admin user
            await SeedAdminUserAsync(context, passwordHasher);

            // Seed default settings
            await SeedSettingsAsync(context);
        }

        private static async Task SeedSettingsAsync(AppDbContext context)
        {
            // Desired default settings. If a setting exists keep it, otherwise create it.
            var desired = new List<Setting>
            {
                new Setting { Id = (int)SettingKey.AllowZeroStock, SettingKey = SettingKey.AllowZeroStock, SettingName = "Allow zero stock", SettingValue = true, Description = "This allow when item stock is zero, the order can still be created. When off, orders cannot be created if any item has zero stock." },
                new Setting { Id = (int)SettingKey.AllowOrdesForLoan, SettingKey = SettingKey.AllowOrdesForLoan, SettingName = "Allow orders for credit", SettingValue = true, Description = "Allows creating orders credit / loan sales. When on orders with negative balance will be marked as Loan." },
                new Setting { Id = (int)SettingKey.AllowCreditOrderWithoutCustomer, SettingKey = SettingKey.AllowCreditOrderWithoutCustomer, SettingName = "Allow credit orders without customer", SettingValue = false, Description = "Allows creating credit orders with out customer (Not recommend)." },
                new Setting { Id = (int)SettingKey.CalculateLoyaltyPointsForCreditOrders, SettingKey = SettingKey.CalculateLoyaltyPointsForCreditOrders, SettingName = "Allow loyalty points for credit orders", SettingValue = false, Description = "Allow to calculate loyalty point for credit orders." },
                new Setting { Id = (int)SettingKey.AlwaysFocusBarcodeField, SettingKey = SettingKey.AlwaysFocusBarcodeField, SettingName = "Keep Focus on Barcode", SettingValue = false, Description = "When this setting is enabled, the cursor always returns to the Barcode field after each scan, allowing continuous scanning without pressing Enter. If you need to change the quantity, you must manually navigate to the Qty field (forexample, using the arrow keys), enter the value, and confirm it.<br/><br/>When this setting is disabled, after scanning a barcode the cursor automatically moves to the Qty field. You can immediately enter the quantity and press Enter to add the item to the order, without manually navigating to the quantity field." },
                new Setting { Id = (int)SettingKey.AllowDeleteOrder, SettingKey = SettingKey.AllowDeleteOrder, SettingName = "Allow delete order", SettingValue = false, Description = "When enabled, users are allowed to delete orders from the system. When disabled, order deletion is prevented by the application (Not recommend).<br/>If AllowZeroStock is enabled, This action increase stock when deleting orders." },
            };

            foreach (var s in desired)
            {
                var existing = await context.Settings.FirstOrDefaultAsync(x => x.SettingKey == s.SettingKey);
                if (existing == null)
                {
                    s.Uuid = Guid.NewGuid().ToString();
                    context.Settings.Add(s);
                }
                else
                {
                    // ensure existing records have SettingName populated
                    if (string.IsNullOrWhiteSpace(existing.SettingName))
                    {
                        existing.SettingName = s.SettingName;
                        context.Entry(existing).State = EntityState.Modified;
                    }
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedRolesAsync(AppDbContext context)
        {
            if (!await context.Roles.AnyAsync())
            {
                var roles = new List<Role>
                {
                    new Role { Id = 1, Name = "SystemAdmin", Uuid = Guid.NewGuid().ToString() },
                    new Role { Id = 2, Name = "ShopAdmin", Uuid = Guid.NewGuid().ToString() },
                    new Role { Id = 3, Name = "Manager", Uuid = Guid.NewGuid().ToString() },
                    new Role { Id = 4, Name = "Cashier", Uuid = Guid.NewGuid().ToString() },
                    new Role { Id = 5, Name = "StockKeeper", Uuid = Guid.NewGuid().ToString() },
                    new Role { Id = 6, Name = "Auditor", Uuid = Guid.NewGuid().ToString() }
                };

                context.Roles.AddRange(roles);
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Seeds the default administrator user if no admin user exists.
        /// Creates a system administrator with default credentials and privileges.
        /// </summary>
        /// <param name="context">The application database context.</param>
        /// <param name="passwordHasher">The password hasher service for securing the admin password.</param>
        private static async Task SeedAdminUserAsync(AppDbContext context, IPasswordHasher passwordHasher)
        {
            if (!await context.Users.AnyAsync(u => u.UserName == "sehandevinda1@gmail.com"))
            {
                var adminUser = new User
                {
                    Id = 1,
                    FirstName = "Sehan",
                    LastName = "devinda",
                    UserName = "sehandevinda1@gmail.com",
                    PasswordHash = passwordHasher.HashPassword("1234"),
                    RoleId = 1, // SystemAdmin role id
                    NIC = "000000000000",
                    Uuid = "312589e9-631c-4511-a65e-b7490179a191"
                };
                context.Users.Add(adminUser);
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Seed default permissions and role mappings
        /// </summary>
        private static async Task SeedPermissionsAsync(AppDbContext context)
        {
            var perms = new List<Permission>
            {
                // Orders
                new Permission { Id = (int)PermissionType.ORDER_VIEW, PermissionType = PermissionType.ORDER_VIEW, PermissionCatagory = PermissionCatagory.ORDER, Description = "Can view orders" },
                new Permission { Id = (int)PermissionType.ORDER_ADD, PermissionType = PermissionType.ORDER_ADD, PermissionCatagory = PermissionCatagory.ORDER, Description = "Can create orders" },
                new Permission { Id = (int)PermissionType.ORDER_UPDATE, PermissionType = PermissionType.ORDER_UPDATE, PermissionCatagory = PermissionCatagory.ORDER, Description = "Can update orders" },
                new Permission { Id = (int)PermissionType.ORDER_DELETE, PermissionType = PermissionType.ORDER_DELETE, PermissionCatagory = PermissionCatagory.ORDER, Description = "Can delete orders" },
                new Permission { Id = (int)PermissionType.ORDER_DELETE_PERMANENTLY, PermissionType = PermissionType.ORDER_DELETE_PERMANENTLY, PermissionCatagory = PermissionCatagory.ORDER, Description = "Can permanently delete orders" },
                new Permission { Id = (int)PermissionType.ORDER_UPDATE_STATUS, PermissionType = PermissionType.ORDER_UPDATE_STATUS, PermissionCatagory = PermissionCatagory.ORDER, Description = "Can update order status" },
                new Permission { Id = (int)PermissionType.ORDER_SETTLEMENT, PermissionType = PermissionType.ORDER_SETTLEMENT, PermissionCatagory = PermissionCatagory.ORDER, Description = "Can record loan settlement payments" },

                // Items
                new Permission { Id = (int)PermissionType.ITEM_VIEW, PermissionType = PermissionType.ITEM_VIEW, PermissionCatagory = PermissionCatagory.ITEM, Description = "Can view items" },
                new Permission { Id = (int)PermissionType.ITEM_ADD, PermissionType = PermissionType.ITEM_ADD, PermissionCatagory = PermissionCatagory.ITEM, Description = "Can create items" },
                new Permission { Id = (int)PermissionType.ITEM_UPDATE, PermissionType = PermissionType.ITEM_UPDATE, PermissionCatagory = PermissionCatagory.ITEM, Description = "Can update items" },
                new Permission { Id = (int)PermissionType.ITEM_DELETE, PermissionType = PermissionType.ITEM_DELETE, PermissionCatagory = PermissionCatagory.ITEM, Description = "Can delete items" },
                new Permission { Id = (int)PermissionType.ITEM_ADD_STOCK, PermissionType = PermissionType.ITEM_ADD_STOCK, PermissionCatagory = PermissionCatagory.ITEM, Description = "Can add stock to items" },

                // Users
                new Permission { Id = (int)PermissionType.USER_VIEW, PermissionType = PermissionType.USER_VIEW, PermissionCatagory = PermissionCatagory.USER, Description = "Can view users" },
                new Permission { Id = (int)PermissionType.USER_CREATE, PermissionType = PermissionType.USER_CREATE, PermissionCatagory = PermissionCatagory.USER, Description = "Can create users" },
                new Permission { Id = (int)PermissionType.USER_UPDATE, PermissionType = PermissionType.USER_UPDATE, PermissionCatagory = PermissionCatagory.USER, Description = "Can update users" },
                new Permission { Id = (int)PermissionType.USER_ACTIVE_STATUS_CHANGE, PermissionType = PermissionType.USER_ACTIVE_STATUS_CHANGE, PermissionCatagory = PermissionCatagory.USER, Description = "Can change user active status users" },
                new Permission { Id = (int)PermissionType.USER_DELETE, PermissionType = PermissionType.USER_DELETE, PermissionCatagory = PermissionCatagory.USER, Description = "Can delete users" },
                new Permission { Id = (int)PermissionType.USER_CHANGE_PASSWORD, PermissionType = PermissionType.USER_CHANGE_PASSWORD, PermissionCatagory = PermissionCatagory.USER, Description = "Can change password" },
                new Permission { Id = (int)PermissionType.USER_MANAGE, PermissionType = PermissionType.USER_MANAGE, PermissionCatagory = PermissionCatagory.USER, Description = "Can manage user accounts and roles" },

                // Suppliers
                new Permission { Id = (int)PermissionType.SUPPLIER_VIEW, PermissionType = PermissionType.SUPPLIER_VIEW, PermissionCatagory = PermissionCatagory.SUPPLIER, Description = "Can view suppliers" },
                new Permission { Id = (int)PermissionType.SUPPLIER_CREATE, PermissionType = PermissionType.SUPPLIER_CREATE, PermissionCatagory = PermissionCatagory.SUPPLIER, Description = "Can create suppliers" },
                new Permission { Id = (int)PermissionType.SUPPLIER_UPDATE, PermissionType = PermissionType.SUPPLIER_UPDATE, PermissionCatagory = PermissionCatagory.SUPPLIER, Description = "Can update suppliers" },
                new Permission { Id = (int)PermissionType.SUPPLIER_DELETE, PermissionType = PermissionType.SUPPLIER_DELETE, PermissionCatagory = PermissionCatagory.SUPPLIER, Description = "Can delete suppliers" },

                // Contacts
                new Permission { Id = (int)PermissionType.CONTACT_VIEW, PermissionType = PermissionType.CONTACT_VIEW, PermissionCatagory = PermissionCatagory.CONTACT, Description = "Can view contacts" },
                new Permission { Id = (int)PermissionType.CONTACT_CREATE, PermissionType = PermissionType.CONTACT_CREATE, PermissionCatagory = PermissionCatagory.CONTACT, Description = "Can create contacts" },
                new Permission { Id = (int)PermissionType.CONTACT_UPDATE, PermissionType = PermissionType.CONTACT_UPDATE, PermissionCatagory = PermissionCatagory.CONTACT, Description = "Can update contacts" },
                new Permission { Id = (int)PermissionType.CONTACT_DELETE, PermissionType = PermissionType.CONTACT_DELETE, PermissionCatagory = PermissionCatagory.CONTACT, Description = "Can delete contacts" },

                // Permissions and roles
                new Permission { Id = (int)PermissionType.PERMISSION_VIEW, PermissionType = PermissionType.PERMISSION_VIEW, PermissionCatagory = PermissionCatagory.PERMISSION, Description = "Can view permissions" },
                new Permission { Id = (int)PermissionType.PERMISSION_ASSIGN, PermissionType = PermissionType.PERMISSION_ASSIGN, PermissionCatagory = PermissionCatagory.PERMISSION, Description = "Can assign permissions to roles" },
                new Permission { Id = (int)PermissionType.ROLE_VIEW, PermissionType = PermissionType.ROLE_VIEW, PermissionCatagory = PermissionCatagory.ROLE, Description = "Can view roles" },
                new Permission { Id = (int)PermissionType.ROLE_CREATE, PermissionType = PermissionType.ROLE_CREATE, PermissionCatagory = PermissionCatagory.ROLE, Description = "Can create roles" },
                new Permission { Id = (int)PermissionType.ROLE_UPDATE, PermissionType = PermissionType.ROLE_UPDATE, PermissionCatagory = PermissionCatagory.ROLE, Description = "Can update roles" },
                new Permission { Id = (int)PermissionType.ROLE_DELETE, PermissionType = PermissionType.ROLE_DELETE, PermissionCatagory = PermissionCatagory.ROLE, Description = "Can delete roles" },

                // Settings
                new Permission { Id = (int)PermissionType.SETTING_MANAGE, PermissionType = PermissionType.SETTING_MANAGE, PermissionCatagory = PermissionCatagory.SETTING, Description = "Can manage settings (on/off)" },

                //Shop details
                new Permission { Id = (int)PermissionType.SHOP_DETAILS_UPDATE, PermissionType = PermissionType.SHOP_DETAILS_UPDATE, PermissionCatagory = PermissionCatagory.SHOP, Description = "Can change shop related details" },

                // Only for Admin
                new Permission { Id = (int)PermissionType.PERMISSION_SYSADMIN_VIEW, PermissionType = PermissionType.PERMISSION_SYSADMIN_VIEW, PermissionCatagory = PermissionCatagory.ROLE, Description = "Can view the SystemAdmin role existence/details" },
            };

            foreach (var p in perms)
            {
                var existing = await context.Permissions.FirstOrDefaultAsync(x => x.PermissionType == p.PermissionType);
                if (existing == null)
                {
                    context.Permissions.Add(p);
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedRolePermissionsAsync(AppDbContext context)
        {
            if (!await context.RolePermissions.AnyAsync())
            {
                // SystemAdmin (roleId 1) gets all permissions
                var allPerms = await context.Permissions.ToListAsync();
                var mappings = allPerms.Select(p => new RolePermission { RoleId = 1, PermissionId = p.Id, Uuid = Guid.NewGuid().ToString() }).ToList(); //SystemAdmin

                // Manager gets a subset
                var managerPermIds = new[] { (int)PermissionType.ORDER_VIEW, (int)PermissionType.ORDER_ADD, (int)PermissionType.ORDER_UPDATE, (int)PermissionType.ORDER_DELETE };
                mappings.AddRange(managerPermIds.Select(id => new RolePermission { RoleId = 3, PermissionId = id, Uuid = Guid.NewGuid().ToString() })); //Manager

                // Cashier gets view & add
                var cashierPermIds = new[] { (int)PermissionType.ORDER_VIEW, (int)PermissionType.ORDER_ADD };
                mappings.AddRange(cashierPermIds.Select(id => new RolePermission { RoleId = 4, PermissionId = id, Uuid = Guid.NewGuid().ToString() })); //Cashier

                context.RolePermissions.AddRange(mappings);
                await context.SaveChangesAsync();
            }
        }

    }
}