using Microsoft.EntityFrameworkCore;
using pos_service.Models;
using pos_service.Models.Audit;
using System.Security.Claims;
using System.Linq;

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
        public DbSet<User> Users                                           { get; set; }
        public DbSet<Contact> Contacts                                     { get; set; }
        public DbSet<Customer> Customers                                   { get; set; }
        public DbSet<Supplier> Suppliers                                   { get; set; }
        public DbSet<Item> Items                                           { get; set; }
        public DbSet<ItemPrice> ItemPrices                                 { get; set; }
        public DbSet<ItemExpiry> ItemExpiries                              { get; set; }
        public DbSet<ItemSupplier> ItemSuppliers                           { get; set; }
        public DbSet<Order> Orders                                         { get; set; }
        public DbSet<OrderItem> OrderItems                                 { get; set; }
        public DbSet<Permission> Permissions                               { get; set; }
        public DbSet<RolePermission> RolePermissions                       { get; set; }
        public DbSet<Role> Roles                                           { get; set; }
        public DbSet<Setting> Settings                                     { get; set; }
        public DbSet<BackupLocation> BackupLocations                       { get; set; }
        public DbSet<BackupHistory> BackupHistories                        { get; set; }
        public DbSet<Shop> Shops                                           { get; set; }
        public DbSet<LoanSettlementLog> LoanSettlementLogs                 { get; set; }
        public DbSet<Inventory> Inventories                                { get; set; }
        public DbSet<InventoryUnit> InventoryUnits                         { get; set; }
        public DbSet<InventoryAdjustAudit> InventoryAdjustAudits           { get; set; }
        public DbSet<ItemPriceAudit> ItemPriceAudits                       { get; set; }


        /// <summary>
        /// This method is used to configure the database model using the Fluent API.
        /// It's where you define keys, relationships, and constraints.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ─── Table Configurations (Industry Standard Explicit Mappings) ──────────
            modelBuilder.Entity<User>().ToTable("tbl_users");
            modelBuilder.Entity<Contact>().ToTable("tbl_contacts");
            modelBuilder.Entity<Customer>().ToTable("tbl_customers");
            modelBuilder.Entity<Supplier>().ToTable("tbl_suppliers");
            modelBuilder.Entity<Item>().ToTable("tbl_items");
            modelBuilder.Entity<ItemPrice>().ToTable("tbl_item_prices");
            modelBuilder.Entity<ItemPriceAudit>().ToTable("tbl_item_price_audits");
            modelBuilder.Entity<ItemExpiry>().ToTable("tbl_item_expiries");
            modelBuilder.Entity<ItemSupplier>().ToTable("tbl_item_suppliers");
            modelBuilder.Entity<Order>().ToTable("tbl_orders");
            modelBuilder.Entity<OrderItem>().ToTable("tbl_order_items");
            modelBuilder.Entity<Permission>().ToTable("tbl_permissions");
            modelBuilder.Entity<RolePermission>().ToTable("tbl_role_permissions");
            modelBuilder.Entity<Role>().ToTable("tbl_roles");
            modelBuilder.Entity<Setting>().ToTable("tbl_settings");
            modelBuilder.Entity<BackupLocation>().ToTable("tbl_backup_locations");
            modelBuilder.Entity<BackupHistory>().ToTable("tbl_backup_histories");
            modelBuilder.Entity<Shop>().ToTable("tbl_shops");
            modelBuilder.Entity<LoanSettlementLog>().ToTable("tbl_loan_settlement_logs");
            modelBuilder.Entity<Inventory>().ToTable("tbl_inventories");
            modelBuilder.Entity<InventoryUnit>().ToTable("tbl_inventory_units");
            modelBuilder.Entity<InventoryAdjustAudit>().ToTable("tbl_inventory_adjust_audits");


            // Add database-side defaults for IAuditable timestamps so the database will populate
            // CreatedAt and UpdatedAt when not supplied by the application.
            foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                         .Where(t => t.ClrType != null && typeof(IAuditable).IsAssignableFrom(t.ClrType)))
            {
                var entity = modelBuilder.Entity(entityType.ClrType);

                // Let the database populate CreatedAt and UpdatedAt using CURRENT_TIMESTAMP.
                // EF will treat these as store-generated values so DB defaults and ON UPDATE apply.
                // Let the database populate CreatedAt and UpdatedAt using CURRENT_TIMESTAMP.
                entity
                    .Property<DateTime>(nameof(IAuditable.CreatedAt))
                    .HasColumnType("datetime(6)")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                    .ValueGeneratedOnAdd();

                entity
                    .Property<DateTime?>(nameof(IAuditable.UpdatedAt))
                    .HasColumnType("datetime(6)")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)")
                    .ValueGeneratedOnAddOrUpdate();

                // CreatedBy is optional and has no DB default; application may set it or leave null.
                entity
                    .Property<string?>(nameof(IAuditable.CreatedBy))
                    .HasColumnType("varchar(36)")
                    .HasMaxLength(255)
                    .IsRequired(false);

                // Link CreatedBy to Users(Uuid) so audit fields reference the user who created the row.
                // On delete set null so that removing a user does not remove the audited record.
                entity
                    .HasOne(typeof(User), null)
                    .WithMany(null)
                    .HasForeignKey(nameof(IAuditable.CreatedBy))
                    .HasPrincipalKey(nameof(User.Uuid))
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);

                // UpdatedBy is optional and has no DB default; application may set it or leave null.
                entity
                    .Property<string?>(nameof(IAuditable.UpdatedBy))
                    .HasColumnType("varchar(36)")
                    .HasMaxLength(255);

                // Link UpdatedBy to Users(Uuid) with the same delete behavior as CreatedBy.
                entity
                    .HasOne(typeof(User), null)
                    .WithMany(null)
                    .HasForeignKey(nameof(IAuditable.UpdatedBy))
                    .HasPrincipalKey(nameof(User.Uuid))
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);

                // IsActive defaults to true
                entity
                    .Property<bool>(nameof(IAuditable.IsActive))
                    .HasColumnType("tinyint(1)")
                    .HasDefaultValue(true)
                    .ValueGeneratedOnAdd();
            }
            // --- Shop Configuration ---
            modelBuilder.Entity<Shop>(entity =>
            {
                // Ensure shop name is unique to avoid duplicate entries with same name.
                entity.HasIndex(s => s.Name).IsUnique();

                entity.Property(s => s.Name).HasMaxLength(255).IsRequired();
                entity.Property(s => s.Address).HasMaxLength(255);
                entity.Property(s => s.PhoneNumber).HasMaxLength(20);
                entity.Property(s => s.Email).HasMaxLength(255);
                entity.Property(s => s.Logo).HasColumnType("mediumblob");

                // Provide alternate key on Uuid like other auditable entities.
                entity.HasAlternateKey(s => s.Uuid);
            });

            // --- Role configuration ---
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasIndex(r => r.Name).IsUnique();

                entity.HasAlternateKey(r => r.Uuid);

                entity.HasOne(r => r.ParentRole)
                      .WithMany(r => r.ChildRoles)
                      .HasForeignKey(r => r.ParentRoleId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(r => r.Permissions)
                      .WithMany()
                      .UsingEntity<RolePermission>(
                          j => j.HasOne(rp => rp.Permission).WithMany().HasForeignKey(rp => rp.PermissionId),
                          j => j.HasOne(rp => rp.Role).WithMany().HasForeignKey(rp => rp.RoleId));
            });

            // --- Permission configuration ---
            modelBuilder.Entity<Permission>(entity =>
            {
                // index on PermissionType to ensure uniqueness of enum mapping
                entity.HasIndex(p => p.PermissionType).IsUnique();

                // store enums as ints
                entity.Property(p => p.PermissionType).HasConversion<string>().HasMaxLength(50);
                entity.Property(p => p.PermissionCatagory).HasConversion<string>().HasMaxLength(50);
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

                // Ensure ProfileImage column uses MEDIUMBLOB (byte[] mapped by EF)
                entity.Property(u => u.ProfileImage)
                      .HasColumnType("mediumblob")
                      .IsRequired(false);
            });

            // --- Supplier Configuration ---
            modelBuilder.Entity<Supplier>(entity =>
            {
                // UUID must be unique
                entity.HasAlternateKey(s => s.Uuid);

                // Supplier name must be unique
                entity.HasIndex(s => s.Name)
                      .IsUnique();

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

                // Make Email unique when provided.
                entity.HasIndex(c => c.Email).IsUnique();

                // 1. Define Uuid as a unique, alternate key.
                // This is REQUIRED to use it as a foreign key target.
                entity.HasAlternateKey(u => u.Uuid);
            });

            // --- Item Configuration ---
            modelBuilder.Entity<Item>(entity =>
            {
                // Define the composite primary key using both Id and SubId.
                entity.HasKey(i => new { i.Id, i.SubId });
                entity.HasOne(i => i.Inventory)
                      .WithOne(inv => inv.Item)
                      .HasForeignKey<Inventory>(inv => inv.ItemUuid)
                      .HasPrincipalKey<Item>(i => i.Uuid)
                      .OnDelete(DeleteBehavior.Cascade);
                // Configure one-to-many to the explicit join entity ItemSupplier.
                entity.HasMany(i => i.ItemSuppliers)
                      .WithOne(isu => isu.Item)
                      .HasForeignKey(isu => new { isu.ItemsId, isu.ItemsSubId })
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(i => i.Price)
                      .WithOne(p => p.Item)
                      .HasForeignKey<ItemPrice>(p => new { p.ItemsId, p.ItemsSubId })
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(i => i.ExpDates)
                      .WithOne(e => e.Item)
                      .HasForeignKey(e => new { e.ItemsId, e.ItemsSubId })
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasAlternateKey(i => i.Uuid);   // creates unique constraint
            });

            modelBuilder.Entity<LoanSettlementLog>(entity =>
            {
                entity.HasOne(ls => ls.Order)
                      .WithMany(o => o.LoanSettlementLogs)
                      .HasForeignKey(ls => ls.OrderId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade);
                // Let the database populate PaymentDate with CURRENT_TIMESTAMP(6) when not supplied
                entity
                    .Property<DateTime>(ls => ls.PaymentDate)
                    .HasColumnType("datetime(6)")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                    .ValueGeneratedOnAdd();

                entity.Property(ls => ls.Status).HasConversion<string>().HasMaxLength(50);
                entity.Property(ls => ls.AmountPaid).HasColumnType("decimal(18,2)");
                entity.Property(ls => ls.RemainingBalance).HasColumnType("decimal(18,2)");
                entity.HasAlternateKey(ls => ls.Uuid);
            });

            modelBuilder.Entity<ItemPrice>(entity =>
            {
                entity.HasKey(p => new { p.ItemsId, p.ItemsSubId });
                entity.Property(p => p.ItemUuid).HasMaxLength(255).IsRequired();
                entity.HasAlternateKey(p => p.ItemUuid);
            });

            modelBuilder.Entity<ItemExpiry>(entity =>
            {
                entity.HasKey(e   => e.Id);
                entity.Property(e => e.ItemUuid).HasMaxLength(255).IsRequired();
            });

            // --- Settings Configuration ---
            modelBuilder.Entity<Setting>(entity =>
            {
                entity.HasIndex(s => s.SettingKey).IsUnique();
                entity.Property(s => s.SettingKey).HasConversion<string>().HasMaxLength(50);
                entity.Property(s => s.Description).HasMaxLength(500);
                entity.HasAlternateKey(s => s.Uuid);
            });

            modelBuilder.Entity<BackupLocation>(entity =>
            {
                entity.HasAlternateKey(b => b.Uuid);
                entity.Property(b => b.Name).HasMaxLength(255).HasColumnType("varchar(255)");
                entity.Property(b => b.Path).HasColumnType("longtext");
                //entity.ToTable("BackupLocations");
            });

            modelBuilder.Entity<BackupHistory>(entity =>
            {
                entity.HasAlternateKey(b => b.Uuid);
                entity.Property(b => b.ScheduleUuid).HasMaxLength(255).HasColumnType("varchar(255)");
                entity.Property(b => b.LocationUuid).HasMaxLength(255).HasColumnType("varchar(255)");
                entity.Property(b => b.Message).HasColumnType("longtext");
                entity.Property(b => b.FilePath).HasColumnType("longtext");
                //entity.ToTable("BackupHistories");
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

            modelBuilder.Entity<Inventory>(entity =>
            {
                entity.HasAlternateKey(i => i.Uuid);
                entity.HasIndex(i => i.ItemUuid).IsUnique();

                entity.Property(i => i.ItemUuid).HasMaxLength(255).IsRequired();
                entity.Property(i => i.StockQuantity).HasColumnType("decimal(18,3)");
                entity.Property(i => i.UnitType).HasConversion<string>().HasMaxLength(50);

                entity.HasOne(i => i.Item)
                      .WithOne(it => it.Inventory)
                      .HasForeignKey<Inventory>(i => i.ItemUuid)
                      .HasPrincipalKey<Item>(it => it.Uuid)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(i => i.Units)
                      .WithOne(u => u.Inventory)
                      .HasForeignKey(u => u.InventoryId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<InventoryUnit>(entity =>
            {
                entity.HasAlternateKey(u => u.Uuid);
                entity.Property(u => u.UnitType).HasConversion<string>().HasMaxLength(50);
                entity.Property(u => u.ParentUnitType).HasConversion<string>().HasMaxLength(50);
                entity.Property(u => u.QuantityPerParent).HasColumnType("decimal(18,3)");
                entity.Property(u => u.QuantityInBaseUnits).HasColumnType("decimal(18,3)");
            });

            modelBuilder.Entity<InventoryAdjustAudit>(entity =>
            {
                entity.HasAlternateKey(a => a.Uuid);
                entity.Property(a => a.InventoryUuid).HasMaxLength(36).IsRequired();
                entity.Property(a => a.ItemUuid).HasMaxLength(36).IsRequired();
                entity.Property(a => a.PreviousQuantity).HasColumnType("decimal(18,3)");
                entity.Property(a => a.NewQuantity).HasColumnType("decimal(18,3)");
                entity.Property(a => a.AdjustmentQuantity).HasColumnType("decimal(18,3)");
                entity.Property(a => a.UnitType).HasConversion<string>().HasMaxLength(50);
                entity.Property(a => a.Comment).HasMaxLength(255);
                entity.Property(a => a.Reason).HasMaxLength(255);

                entity.HasOne(a => a.Inventory)
                      .WithMany()
                      .HasForeignKey(a => a.InventoryUuid)
                      .HasPrincipalKey(i => i.Uuid)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade);
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
                entity.Property(o => o.MainStatus).HasConversion<string>().HasMaxLength(50);
                entity.Property(o => o.SubStatus).HasConversion<string>().HasMaxLength(50);
                entity.Property(o => o.PaymentMethod).HasConversion<string>().HasMaxLength(50);
                entity.Property(o => o.SaleType).HasConversion<string>().HasMaxLength(50);

                // Configure the relationship to the User (Cashier).
                entity.HasOne(o => o.Cashier)
                      .WithMany() // A User can have many Orders, but we don't need a navigation property on User.
                      .HasForeignKey(o => o.CashierId)
                      .OnDelete(DeleteBehavior.SetNull); // do not delete set as null

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



            // --- ReturnedItemsSummary DB view mapping (keyless) ---
            modelBuilder.Entity<ReturnedItemsSummary>(eb =>
            {
                eb.HasNoKey();
                eb.ToView("view_returned_items_summary");
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
                string userUuid = null;
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
                    // ignore and use null
                }

                // Only set non-timestamp audit fields here. CreatedAt/UpdatedAt are DB-generated.
                auditableEntity.UpdatedBy = userUuid;

                if (entityEntry.State == EntityState.Added)
                {
                    if (auditableEntity.Uuid == null) auditableEntity.Uuid = Guid.NewGuid().ToString();
                    auditableEntity.CreatedBy = userUuid;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
