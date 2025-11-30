using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using pos_service.Models;
using pos_service.Models.Enums;
using pos_service.Security;
using pos_service.Services;

namespace pos_service.Data
{
    public class AppDbContext : DbContext
    {
        private readonly ICurrentUserService _currentUserService;

        public AppDbContext(
            DbContextOptions<AppDbContext> options,
            ICurrentUserService currentUserService) : base(options)
        {
            _currentUserService = currentUserService;
        }

        // Define a DbSet for each of your models.
        // This tells EF Core to create a table for each one.
        public DbSet<User> Users           { get; set; }
        public DbSet<Contact> Contacts     { get; set; }
        public DbSet<Customer> Customers   { get; set; }
        public DbSet<Supplier> Suppliers   { get; set; }
        public DbSet<Item> Items           { get; set; }
        public DbSet<Order> Orders         { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        /// <summary>
        /// This method is used to configure the database model using the Fluent API.
        /// It's where you define keys, relationships, and constraints.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- User Configuration ---
            modelBuilder.Entity<User>(entity =>
            {
                // Make the UserName field a unique index to prevent duplicate usernames.
                entity.HasIndex(u => u.UserName).IsUnique();

                // Store the UserRole enum as a string in the database for readability.
                entity.Property(u => u.Role).HasConversion<string>();

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

                // Configure the many-to-many relationship between Item and Supplier.
                // EF Core will create a junction table automatically.
                entity.HasMany(i => i.Suppliers)
                      .WithMany(s => s.Items);

                entity.HasAlternateKey(i => i.Uuid);   // creates unique constraint
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
                        e.State == EntityState.Added
                        || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                var auditableEntity = (IAuditable)entityEntry.Entity;

                // Get current user from the service
                var currentUser = _currentUserService.GetCurrentUser();
                var userUuid = currentUser?.IsAuthenticated == true ? currentUser.Uuid : "SYSTEM";

                auditableEntity.UpdatedAt = DateTime.UtcNow;
                auditableEntity.UpdatedBy = userUuid;

                if (entityEntry.State == EntityState.Added)
                {
                    auditableEntity.Uuid = Guid.NewGuid().ToString();
                    auditableEntity.CreatedAt = DateTime.UtcNow;
                    auditableEntity.CreatedBy = userUuid;
                    auditableEntity.IsActive = true;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
