using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pos_service.Migrations
{
    /// <inheritdoc />
    public partial class update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Customers_CustomerId1",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_CustomerId1",
                table: "Orders");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DropColumn(
                name: "IsRetailSale",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OriginalItemId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "OriginalItemSubId",
                table: "OrderItems");

            migrationBuilder.RenameColumn(
                name: "CustomerId1",
                table: "Orders",
                newName: "SaleType");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "OrderItems",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "OrderItems",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "OrderItems",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "OriginalItemUuid",
                table: "OrderItems",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "OrderItems",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "OrderItems",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "Uuid",
                table: "OrderItems",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Suppliers_Uuid",
                table: "Suppliers",
                column: "Uuid");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Orders_Uuid",
                table: "Orders",
                column: "Uuid");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_OrderItems_Uuid",
                table: "OrderItems",
                column: "Uuid");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Items_Uuid",
                table: "Items",
                column: "Uuid");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Contacts_Uuid",
                table: "Contacts",
                column: "Uuid");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OriginalItemUuid",
                table: "OrderItems",
                column: "OriginalItemUuid");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Items_OriginalItemUuid",
                table: "OrderItems",
                column: "OriginalItemUuid",
                principalTable: "Items",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Items_OriginalItemUuid",
                table: "OrderItems");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Suppliers_Uuid",
                table: "Suppliers");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Orders_Uuid",
                table: "Orders");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_OrderItems_Uuid",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_OriginalItemUuid",
                table: "OrderItems");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Items_Uuid",
                table: "Items");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Contacts_Uuid",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "OriginalItemUuid",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "Uuid",
                table: "OrderItems");

            migrationBuilder.RenameColumn(
                name: "SaleType",
                table: "Orders",
                newName: "CustomerId1");

            migrationBuilder.AddColumn<bool>(
                name: "IsRetailSale",
                table: "Orders",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "OriginalItemId",
                table: "OrderItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OriginalItemSubId",
                table: "OrderItems",
                type: "int",
                nullable: true);

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "FirstName", "IsActive", "LastName", "NIC", "PasswordHash", "ProfileImageUrl", "Role", "UpdatedAt", "UpdatedBy", "UserName", "Uuid" },
                values: new object[] { 1, new DateTime(2025, 10, 25, 18, 39, 1, 741, DateTimeKind.Utc).AddTicks(7995), "System Seed", "System", true, "Admin", "000000000000", "AQAAAAIAAYagAAAAEK7H8Ro9ULXE9rLzW29GsFOR4QKdBPsS7WKwLcAf1B+btUee9ZEOi9xFIFv313doLg==", null, "SystemAdmin", null, null, "admin@pos.com", new Guid("c69a5ced-7be4-4df0-93d6-3276a431c3f3") });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerId1",
                table: "Orders",
                column: "CustomerId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Customers_CustomerId1",
                table: "Orders",
                column: "CustomerId1",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
