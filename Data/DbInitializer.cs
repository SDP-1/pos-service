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
    }
}