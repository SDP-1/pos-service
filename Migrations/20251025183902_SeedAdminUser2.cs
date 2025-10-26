using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pos_service.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdminUser2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Orders_OrderId1",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_OrderId1",
                table: "OrderItems");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DropColumn(
                name: "OrderId1",
                table: "OrderItems");

            migrationBuilder.AddColumn<int>(
                name: "CustomerId1",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
                name: "CustomerId1",
                table: "Orders");

            migrationBuilder.AddColumn<int>(
                name: "OrderId1",
                table: "OrderItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "FirstName", "IsActive", "LastName", "NIC", "PasswordHash", "ProfileImageUrl", "Role", "UpdatedAt", "UpdatedBy", "UserName", "Uuid" },
                values: new object[] { 1, new DateTime(2025, 10, 25, 18, 33, 22, 312, DateTimeKind.Utc).AddTicks(1810), "System Seed", "System", true, "Admin", "000000000000", "AQAAAAIAAYagAAAAEFPPI1taFKsJcCZOquN20vSU09LesmnVjDhJjsCl5FJ48TjggspVkCbVRwLE3PicrQ==", null, "SystemAdmin", null, null, "admin@pos.com", new Guid("09ab7fb9-e1ee-47e7-8f61-04763ba822cb") });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId1",
                table: "OrderItems",
                column: "OrderId1");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Orders_OrderId1",
                table: "OrderItems",
                column: "OrderId1",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
