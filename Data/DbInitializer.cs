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

            // Seed default suppliers
            await SeedSuppliersAsync(context);

            // Seed default items
            await SeedItemsAsync(context);
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
            if (!await context.Users.AnyAsync(u => u.UserName == "admin@pos.com"))
            {
                var adminUser = new User
                {
                    Id = 1,
                    FirstName = "System",
                    LastName = "Admin",
                    UserName = "admin@pos.com",
                    PasswordHash = passwordHasher.HashPassword("AdminPass@123"),
                    RoleId = 1, // SystemAdmin role id
                    NIC = "000000000000",
                    Uuid = Guid.NewGuid().ToString(),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System Seed"
                };
                context.Users.Add(adminUser);
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Seeds default suppliers if no suppliers exist in the database.
        /// Creates initial supplier records for the POS system.
        /// </summary>
        /// <param name="context">The application database context.</param>
        private static async Task SeedSuppliersAsync(AppDbContext context)
        {
            if (!await context.Suppliers.AnyAsync())
            {
                context.Suppliers.AddRange(
                    new Supplier { Id = 1, Name = "Default Supplier 1", Uuid = Guid.NewGuid().ToString() },
                    new Supplier { Id = 2, Name = "Default Supplier 2", Uuid = Guid.NewGuid().ToString() }
                );
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Seeds default items if no items exist in the database.
        /// Creates initial item records with associated suppliers for the POS system.
        /// </summary>
        /// <param name="context">The application database context.</param>
        private static async Task SeedItemsAsync(AppDbContext context)
        {
            if (!await context.Items.AnyAsync())
            {
                // Get existing suppliers from DB
                var suppliers = await context.Suppliers.ToListAsync();
                var supplier = await context.Suppliers.FindAsync(1); // supplierId is int
                if (supplier == null)
                    throw new Exception("Supplier not found");

                context.Items.AddRange(
                    new Item { Id = 1, SubId = 0, Name = "Item 1", PrintName = "Item 1", RetailPrice = 100, Uuid = Guid.NewGuid().ToString(), Suppliers = new List<Supplier>() { supplier } },
                    new Item { Id = 2, SubId = 0, Name = "Item 2", PrintName = "Item 2", RetailPrice = 200, Uuid = Guid.NewGuid().ToString(), Suppliers = suppliers }
                );
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Seed default permissions and role mappings
        /// </summary>
        private static async Task SeedPermissionsAsync(AppDbContext context)
        {
            if (!await context.Permissions.AnyAsync())
            {
                var perms = new List<Permission>
                {
                    // Orders
                    new Permission { Id = (int)PermissionType.ORDER_VIEW, Name = "ORDER_VIEW", Description = "Can view orders", Uuid = Guid.NewGuid().ToString() },
                    new Permission { Id = (int)PermissionType.ORDER_ADD, Name = "ORDER_ADD", Description = "Can create orders", Uuid = Guid.NewGuid().ToString() },
                    new Permission { Id = (int)PermissionType.ORDER_UPDATE, Name = "ORDER_UPDATE", Description = "Can update orders", Uuid = Guid.NewGuid().ToString() },
                    new Permission { Id = (int)PermissionType.ORDER_DELETE, Name = "ORDER_DELETE", Description = "Can delete orders", Uuid = Guid.NewGuid().ToString() },
                    new Permission { Id = (int)PermissionType.ORDER_DELETE_PERMANENTLY, Name = "ORDER_DELETE_PERMANENTLY", Description = "Can permanently delete orders", Uuid = Guid.NewGuid().ToString() },
                    new Permission { Id = (int)PermissionType.ORDER_UPDATE_STATUS, Name = "ORDER_UPDATE_STATUS", Description = "Can update order status", Uuid = Guid.NewGuid().ToString() },

                    // Items
                    new Permission { Id = (int)PermissionType.ITEM_VIEW, Name = "ITEM_VIEW", Description = "Can view items", Uuid = Guid.NewGuid().ToString() },
                    new Permission { Id = (int)PermissionType.ITEM_ADD, Name = "ITEM_ADD", Description = "Can create items", Uuid = Guid.NewGuid().ToString() },
                    new Permission { Id = (int)PermissionType.ITEM_UPDATE, Name = "ITEM_UPDATE", Description = "Can update items", Uuid = Guid.NewGuid().ToString() },
                    new Permission { Id = (int)PermissionType.ITEM_DELETE, Name = "ITEM_DELETE", Description = "Can delete items", Uuid = Guid.NewGuid().ToString() },
                    new Permission { Id = (int)PermissionType.ITEM_ADD_STOCK, Name = "ITEM_ADD_STOCK", Description = "Can add stock to items", Uuid = Guid.NewGuid().ToString() },

                    // Users
                    new Permission { Id = (int)PermissionType.USER_VIEW, Name = "USER_VIEW", Description = "Can view users", Uuid = Guid.NewGuid().ToString() },
                    new Permission { Id = (int)PermissionType.USER_CREATE, Name = "USER_CREATE", Description = "Can create users", Uuid = Guid.NewGuid().ToString() },
                    new Permission { Id = (int)PermissionType.USER_UPDATE, Name = "USER_UPDATE", Description = "Can update users", Uuid = Guid.NewGuid().ToString() },
                    new Permission { Id = (int)PermissionType.USER_DEACTIVATE, Name = "USER_DEACTIVATE", Description = "Can deactivate users", Uuid = Guid.NewGuid().ToString() },
                    new Permission { Id = (int)PermissionType.USER_DELETE, Name = "USER_DELETE", Description = "Can delete users", Uuid = Guid.NewGuid().ToString() },
                    new Permission { Id = (int)PermissionType.USER_CHANGE_PASSWORD, Name = "USER_CHANGE_PASSWORD", Description = "Can change password", Uuid = Guid.NewGuid().ToString() },
                    new Permission { Id = (int)PermissionType.USER_MANAGE, Name = "USER_MANAGE", Description = "Can manage user accounts and roles", Uuid = Guid.NewGuid().ToString() },

                    // Suppliers
                    new Permission { Id = (int)PermissionType.SUPPLIER_VIEW, Name = "SUPPLIER_VIEW", Description = "Can view suppliers", Uuid = Guid.NewGuid().ToString() },
                    new Permission { Id = (int)PermissionType.SUPPLIER_CREATE, Name = "SUPPLIER_CREATE", Description = "Can create suppliers", Uuid = Guid.NewGuid().ToString() },
                    new Permission { Id = (int)PermissionType.SUPPLIER_UPDATE, Name = "SUPPLIER_UPDATE", Description = "Can update suppliers", Uuid = Guid.NewGuid().ToString() },
                    new Permission { Id = (int)PermissionType.SUPPLIER_DELETE, Name = "SUPPLIER_DELETE", Description = "Can delete suppliers", Uuid = Guid.NewGuid().ToString() },

                    // Contacts
                    new Permission { Id = (int)PermissionType.CONTACT_VIEW, Name = "CONTACT_VIEW", Description = "Can view contacts", Uuid = Guid.NewGuid().ToString() },
                    new Permission { Id = (int)PermissionType.CONTACT_CREATE, Name = "CONTACT_CREATE", Description = "Can create contacts", Uuid = Guid.NewGuid().ToString() },
                    new Permission { Id = (int)PermissionType.CONTACT_UPDATE, Name = "CONTACT_UPDATE", Description = "Can update contacts", Uuid = Guid.NewGuid().ToString() },
                    new Permission { Id = (int)PermissionType.CONTACT_DELETE, Name = "CONTACT_DELETE", Description = "Can delete contacts", Uuid = Guid.NewGuid().ToString() },

                    // Permissions and roles
                    new Permission { Id = (int)PermissionType.PERMISSION_VIEW, Name = "PERMISSION_VIEW", Description = "Can view permissions", Uuid = Guid.NewGuid().ToString() },
                    new Permission { Id = (int)PermissionType.PERMISSION_ASSIGN, Name = "PERMISSION_ASSIGN", Description = "Can assign permissions to roles", Uuid = Guid.NewGuid().ToString() },
                    new Permission { Id = (int)PermissionType.ROLE_VIEW, Name = "ROLE_VIEW", Description = "Can view roles", Uuid = Guid.NewGuid().ToString() },
                    new Permission { Id = (int)PermissionType.ROLE_CREATE, Name = "ROLE_CREATE", Description = "Can create roles", Uuid = Guid.NewGuid().ToString() },
                    new Permission { Id = (int)PermissionType.ROLE_UPDATE, Name = "ROLE_UPDATE", Description = "Can update roles", Uuid = Guid.NewGuid().ToString() },
                    new Permission { Id = (int)PermissionType.ROLE_DELETE, Name = "ROLE_DELETE", Description = "Can delete roles", Uuid = Guid.NewGuid().ToString() }
                };

                context.Permissions.AddRange(perms);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedRolePermissionsAsync(AppDbContext context)
        {
            if (!await context.RolePermissions.AnyAsync())
            {
                // SystemAdmin (roleId 1) gets all permissions
                var allPerms = await context.Permissions.ToListAsync();
                var mappings = allPerms.Select(p => new RolePermission { RoleId = 1, PermissionId = p.Id, Uuid = Guid.NewGuid().ToString() }).ToList();

                // Manager gets a subset
                var managerPermIds = new[] { (int)PermissionType.ORDER_VIEW, (int)PermissionType.ORDER_ADD, (int)PermissionType.ORDER_UPDATE, (int)PermissionType.ORDER_DELETE };
                mappings.AddRange(managerPermIds.Select(id => new RolePermission { RoleId = 3, PermissionId = id, Uuid = Guid.NewGuid().ToString() }));

                // Cashier gets view & add
                var cashierPermIds = new[] { (int)PermissionType.ORDER_VIEW, (int)PermissionType.ORDER_ADD };
                mappings.AddRange(cashierPermIds.Select(id => new RolePermission { RoleId = 4, PermissionId = id, Uuid = Guid.NewGuid().ToString() }));

                context.RolePermissions.AddRange(mappings);
                await context.SaveChangesAsync();
            }
        }
    }
}