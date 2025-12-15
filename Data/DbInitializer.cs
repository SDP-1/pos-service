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

                var item1 = new Item
                {
                    Id = 1,
                    SubId = 0,
                    Name = "Item 1",
                    StockQuantity = 200,
                    PrintName = "Item 1",
                    RetailPrice = 100,
                    BuyingPrice = 90,
                    MarkedPrice = 100,
                    Uuid = Guid.NewGuid().ToString(),
                    ItemSuppliers = new List<ItemSupplier>()
                    {
                        new ItemSupplier { SuppliersId = supplier.Id, ItemsId = 1, ItemsSubId = 0,Uuid = Guid.NewGuid().ToString(), Supplier = supplier }
                    }
                };

                var item2 = new Item
                {
                    Id = 2,
                    SubId = 0,
                    Name = "Item 2",
                    StockQuantity = 200,
                    PrintName = "Item 2",
                    RetailPrice = 200,
                    BuyingPrice = 90,
                    MarkedPrice = 100,
                    Uuid = Guid.NewGuid().ToString(),
                    ItemSuppliers = suppliers.Select(s => new ItemSupplier { SuppliersId = s.Id, ItemsId = 2, Uuid = Guid.NewGuid().ToString(), ItemsSubId = 0, Supplier = s }).ToList()
                };

                context.Items.AddRange(item1, item2);
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
                    new Permission { Id = (int)PermissionType.ORDER_VIEW, PermissionType = PermissionType.ORDER_VIEW, PermissionCatagory = PermissionCatagory.ORDER, Description = "Can view orders" },
                    new Permission { Id = (int)PermissionType.ORDER_ADD, PermissionType = PermissionType.ORDER_ADD, PermissionCatagory = PermissionCatagory.ORDER, Description = "Can create orders" },
                    new Permission { Id = (int)PermissionType.ORDER_UPDATE, PermissionType = PermissionType.ORDER_UPDATE, PermissionCatagory = PermissionCatagory.ORDER, Description = "Can update orders" },
                    new Permission { Id = (int)PermissionType.ORDER_DELETE, PermissionType = PermissionType.ORDER_DELETE, PermissionCatagory = PermissionCatagory.ORDER, Description = "Can delete orders" },
                    new Permission { Id = (int)PermissionType.ORDER_DELETE_PERMANENTLY, PermissionType = PermissionType.ORDER_DELETE_PERMANENTLY, PermissionCatagory = PermissionCatagory.ORDER, Description = "Can permanently delete orders" },
                    new Permission { Id = (int)PermissionType.ORDER_UPDATE_STATUS, PermissionType = PermissionType.ORDER_UPDATE_STATUS, PermissionCatagory = PermissionCatagory.ORDER, Description = "Can update order status" },

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
                    new Permission { Id = (int)PermissionType.USER_DEACTIVATE, PermissionType = PermissionType.USER_DEACTIVATE, PermissionCatagory = PermissionCatagory.USER, Description = "Can deactivate users" },
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
                   

                    // Only for Admin
                    new Permission { Id = (int)PermissionType.PERMISSION_SYSADMIN_VIEW, PermissionType = PermissionType.PERMISSION_SYSADMIN_VIEW, PermissionCatagory = PermissionCatagory.ROLE, Description = "Can view the SystemAdmin role existence/details" },
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