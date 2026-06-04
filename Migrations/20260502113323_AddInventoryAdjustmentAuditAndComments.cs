using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pos_service.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryAdjustmentAuditAndComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "Inventories",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "Inventories",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "InventoryAdjustAudits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    InventoryUuid = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ItemUuid = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PreviousQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    NewQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    AdjustmentQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    UnitType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Increase = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Comment = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Reason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Uuid = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
                    CreatedBy = table.Column<string>(type: "varchar(36)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedBy = table.Column<string>(type: "varchar(36)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryAdjustAudits", x => x.Id);
                    table.UniqueConstraint("AK_InventoryAdjustAudits_Uuid", x => x.Uuid);
                    table.ForeignKey(
                        name: "FK_InventoryAdjustAudits_Inventories_InventoryUuid",
                        column: x => x.InventoryUuid,
                        principalTable: "Inventories",
                        principalColumn: "Uuid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryAdjustAudits_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Uuid",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InventoryAdjustAudits_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Uuid",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAdjustAudits_CreatedBy",
                table: "InventoryAdjustAudits",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAdjustAudits_InventoryUuid",
                table: "InventoryAdjustAudits",
                column: "InventoryUuid");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAdjustAudits_UpdatedBy",
                table: "InventoryAdjustAudits",
                column: "UpdatedBy");

            // Create the audit trigger for inventory adjustments
            migrationBuilder.Sql(@"
                DELIMITER $$

                CREATE TRIGGER `trg_inventories_audit_on_update`
                AFTER UPDATE ON `inventories`
                FOR EACH ROW
                BEGIN
                    DECLARE adjustment_qty DECIMAL(18,3);
                    DECLARE is_increase TINYINT(1);
                    DECLARE new_uuid VARCHAR(36);

                    -- Check if StockQuantity has changed
                    IF NEW.StockQuantity != OLD.StockQuantity THEN
                        -- Calculate adjustment quantity and direction
                        SET adjustment_qty = NEW.StockQuantity - OLD.StockQuantity;
                        SET is_increase = IF(adjustment_qty > 0, 1, 0);
                        SET new_uuid = UUID();

                        -- Insert audit record
                        INSERT INTO `inventoryAdjustAudits` (
                            `InventoryUuid`,
                            `ItemUuid`,
                            `PreviousQuantity`,
                            `NewQuantity`,
                            `AdjustmentQuantity`,
                            `UnitType`,
                            `Increase`,
                            `Comment`,
                            `Reason`,
                            `Uuid`,
                            `CreatedAt`,
                            `CreatedBy`,
                            `IsActive`
                        ) VALUES (
                            NEW.Uuid,
                            NEW.ItemUuid,
                            OLD.StockQuantity,
                            NEW.StockQuantity,
                            adjustment_qty,
                            NEW.UnitType,
                            is_increase,
                            NEW.Comment,
                            NEW.Reason,
                            new_uuid,
                            CURRENT_TIMESTAMP(6),
                            NEW.UpdatedBy,
                            1
                        );
                    END IF;
                END$$

                DELIMITER ;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the trigger first
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS `trg_inventories_audit_on_update`;");

            migrationBuilder.DropTable(
                name: "InventoryAdjustAudits");

            migrationBuilder.DropColumn(
                name: "Comment",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "Inventories");
        }
    }
}
