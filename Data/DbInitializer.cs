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

            // Seed admin user
            await SeedAdminUserAsync(context, passwordHasher);

            // Seed default suppliers
            await SeedSuppliersAsync(context);

            // Seed default items
            await SeedItemsAsync(context);

            // Seed permissions and role mappings
            await SeedPermissionsAsync(context);
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
                    Role = UserRole.SystemAdmin,
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
                    new Permission { Name = "VIEW_ORDER", Description = "Can view orders", Uuid = Guid.NewGuid().ToString() },
                    new Permission { Name = "ADD_ORDER", Description = "Can create orders", Uuid = Guid.NewGuid().ToString() },
                    new Permission { Name = "UPDATE_ORDER", Description = "Can update orders", Uuid = Guid.NewGuid().ToString() },
                    new Permission { Name = "DELETE_ORDER", Description = "Can delete orders", Uuid = Guid.NewGuid().ToString() },
                    new Permission { Name = "MANAGE_USERS", Description = "Can manage user accounts and roles", Uuid = Guid.NewGuid().ToString() }
                };

                context.Permissions.AddRange(perms);
                await context.SaveChangesAsync();

                var viewer      = perms.Single(p => p.Name == "VIEW_ORDER");
                var adder       = perms.Single(p => p.Name == "ADD_ORDER");
                var updater     = perms.Single(p => p.Name == "UPDATE_ORDER");
                var deleter     = perms.Single(p => p.Name == "DELETE_ORDER");
                var manageUsers = perms.Single(p => p.Name == "MANAGE_USERS");

                // Default mapping for roles
                var mappings = new List<RolePermission>
                {
                    new RolePermission { Role = UserRole.Cashier, PermissionId = viewer.Id, Uuid = Guid.NewGuid().ToString() },
                    new RolePermission { Role = UserRole.Cashier, PermissionId = adder.Id, Uuid = Guid.NewGuid().ToString() },
                    new RolePermission { Role = UserRole.Manager, PermissionId = viewer.Id, Uuid = Guid.NewGuid().ToString() },
                    new RolePermission { Role = UserRole.Manager, PermissionId = adder.Id, Uuid = Guid.NewGuid().ToString() },
                    new RolePermission { Role = UserRole.Manager, PermissionId = updater.Id, Uuid = Guid.NewGuid().ToString() },
                    new RolePermission { Role = UserRole.Manager, PermissionId = deleter.Id, Uuid = Guid.NewGuid().ToString() },

                    new RolePermission { Role = UserRole.SystemAdmin, PermissionId = viewer.Id, Uuid = Guid.NewGuid().ToString() },
                    new RolePermission { Role = UserRole.SystemAdmin, PermissionId = adder.Id, Uuid = Guid.NewGuid().ToString() },
                    new RolePermission { Role = UserRole.SystemAdmin, PermissionId = updater.Id, Uuid = Guid.NewGuid().ToString() },
                    new RolePermission { Role = UserRole.SystemAdmin, PermissionId = deleter.Id, Uuid = Guid.NewGuid().ToString() },
                    new RolePermission { Role = UserRole.SystemAdmin, PermissionId = manageUsers.Id, Uuid = Guid.NewGuid().ToString() }
                };

                context.RolePermissions.AddRange(mappings);
                await context.SaveChangesAsync();
            }
        }
    }
}