using Microsoft.EntityFrameworkCore;
using pos_service.Models;
using pos_service.Models.Audit;
using System.Security.Claims;

namespace pos_service.Data
{
    public class AppDbContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AppDbContext(
            DbContextOptions<AppDbContext> options,
            IHttpContextAccessor httpContextAccessor) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // Define a DbSet for each of your models.
        // This tells EF Core to create a table for each one.
        public DbSet<User> Users           { get; set; }
        public DbSet<Contact> Contacts     { get; set; }
        public DbSet<Customer> Customers   { get; set; }
        public DbSet<Supplier> Suppliers   { get; set; }
        public DbSet<Item> Items           { get; set; }
        public DbSet<ItemSupplier> ItemSuppliers { get; set; }
        public DbSet<Order> Orders         { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Role> Roles { get; set; }

        /// <summary>
        /// This method is used to configure the database model using the Fluent API.
        /// It's where you define keys, relationships, and constraints.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Role configuration ---
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasIndex(r => r.Name).IsUnique();

                entity.HasAlternateKey(r => r.Uuid);
            });

            // --- Permission configuration ---
            modelBuilder.Entity<Permission>(entity =>
            {
                // index on PermissionType to ensure uniqueness of enum mapping
                entity.HasIndex(p => p.PermissionType).IsUnique();

                // store enums as ints
                entity.Property(p => p.PermissionType).HasConversion<string>();
                entity.Property(p => p.PermissionCatagory).HasConversion<string>();
            });

            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.HasOne(rp => rp.Permission)
                      .WithMany()
                      .HasForeignKey(rp => rp.PermissionId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(rp => rp.Role)
                      .WithMany()
                      .HasForeignKey(rp => rp.RoleId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasAlternateKey(rp => rp.Uuid);
            });

            // --- User Configuration ---
            modelBuilder.Entity<User>(entity =>
            {
                // Make the UserName field a unique index to prevent duplicate usernames.
                entity.HasIndex(u => u.UserName).IsUnique();

                // relationship between User and Role
                entity.HasOne(u => u.Role)
                      .WithMany() // removed inverse Users navigation
                      .HasForeignKey(u => u.RoleId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Restrict);

                // User -> Contacts (cascade)
                entity.HasMany(s => s.Contacts)
                      .WithOne(c => c.User)
                      .HasForeignKey(c => c.UserId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.Cascade);

                // 1. Define Uuid as a unique, alternate key.
                // This is REQUIRED to use it as a foreign key target.
                entity.HasAlternateKey(u => u.Uuid);
            });

            // --- Supplier Configuration ---
            modelBuilder.Entity<Supplier>(entity =>
            {
                // UUID must be unique
                entity.HasAlternateKey(s => s.Uuid);

                // Supplier -> Contacts (cascade)
                entity.HasMany(s => s.Contacts)
                      .WithOne(c => c.Supplier)
                      .HasForeignKey(c => c.SupplierId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // --- Customer Configuration ---
            modelBuilder.Entity<Customer>(entity =>
            {
                // Make the PhoneNumber unique as it's the primary identifier for customers.
                entity.HasIndex(c => c.PhoneNumber).IsUnique();

                // 1. Define Uuid as a unique, alternate key.
                // This is REQUIRED to use it as a foreign key target.
                entity.HasAlternateKey(u => u.Uuid);
            });

            // --- Item Configuration ---
            modelBuilder.Entity<Item>(entity =>
            {
                // Define the composite primary key using both Id and SubId.
                entity.HasKey(i => new { i.Id, i.SubId });
                // Configure one-to-many to the explicit join entity ItemSupplier.
                entity.HasMany(i => i.ItemSuppliers)
                      .WithOne(isu => isu.Item)
                      .HasForeignKey(isu => new { isu.ItemsId, isu.ItemsSubId })
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasAlternateKey(i => i.Uuid);   // creates unique constraint
            });

            // --- ItemSupplier Configuration ---
            modelBuilder.Entity<ItemSupplier>(entity =>
            {
                entity.HasKey(e => new { e.SuppliersId, e.ItemsId, e.ItemsSubId });

                entity.HasOne(e => e.Supplier)
                      .WithMany(s => s.ItemSuppliers)
                      .HasForeignKey(e => e.SuppliersId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Item)
                      .WithMany(i => i.ItemSuppliers)
                      .HasForeignKey(e => new { e.ItemsId, e.ItemsSubId })
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasAlternateKey(e => e.Uuid);
            });

            modelBuilder.Entity<Contact>(entity =>
            {
                // UUID must be unique
                entity.HasAlternateKey(c => c.Uuid);
            });

            // --- Order Configuration ---
            modelBuilder.Entity<Order>(entity =>
            {
                // Make the OrderNumber unique.
                entity.HasIndex(o => o.OrderNumber).IsUnique();

                // Convert enums to strings for readability in the database.
                entity.Property(o => o.Status).HasConversion<string>();
                entity.Property(o => o.PaymentMethod).HasConversion<string>();
                entity.Property(o => o.SaleType).HasConversion<string>();

                // Configure the relationship to the User (Cashier).
                entity.HasOne(o => o.Cashier)
                      .WithMany() // A User can have many Orders, but we don't need a navigation property on User.
                      .HasForeignKey(o => o.CashierId);

                // Configure the optional relationship to the Customer.
                entity.HasOne(o => o.Customer)
                      .WithMany(c => c.Orders)
                      .HasForeignKey(o => o.CustomerId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.SetNull); // do not delete set as null

                entity.HasAlternateKey(i => i.Uuid);   // creates unique constraint
            });

            // --- OrderItem Configuration ---
            modelBuilder.Entity<OrderItem>(entity =>
            {
                // Configure the required relationship to the parent Order.
                // Explicitly use the 'Order' navigation property on the OrderItem side
                entity.HasOne(oi => oi.Order)
                      .WithMany(o => o.OrderItems)
                      .HasForeignKey(oi => oi.OrderId)
                      .IsRequired();

                entity.HasOne(oi => oi.Item)
                      .WithMany()
                      .HasForeignKey(oi => oi.OriginalItemUuid)
                      .HasPrincipalKey(i => i.Uuid)
                      .OnDelete(DeleteBehavior.SetNull); // do not delete set as null

                entity.HasAlternateKey(i => i.Uuid);   // creates unique constraint
            });
        }

        /// <summary>
        /// Overriding SaveChangesAsync to automatically set the IAuditable properties.
        /// This code runs every time you save data to the database.
        /// </summary>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is IAuditable && (
                            e.State == EntityState.Added || 
                            e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                var auditableEntity = (IAuditable)entityEntry.Entity;

                // Get current user from the HttpContext accessor
                string userUuid = "SYSTEM";
                try
                {
                    var principal = _httpContextAccessor?.HttpContext?.User;
                    if (principal?.Identity?.IsAuthenticated == true)
                    {
                        var uuidClaim = principal.FindFirst("uuid") ?? principal.FindFirst(ClaimTypes.NameIdentifier);
                        if (uuidClaim != null && !string.IsNullOrEmpty(uuidClaim.Value))
                        {
                            userUuid = uuidClaim.Value;
                        }
                    }
                }
                catch
                {
                    // ignore and use SYSTEM
                }

                auditableEntity.UpdatedAt = DateTime.UtcNow;
                auditableEntity.UpdatedBy = userUuid;

                if (entityEntry.State == EntityState.Added)
                {
                    if(auditableEntity.Uuid == null) auditableEntity.Uuid = Guid.NewGuid().ToString();
                    auditableEntity.CreatedAt = DateTime.UtcNow;
                    auditableEntity.CreatedBy = userUuid;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
