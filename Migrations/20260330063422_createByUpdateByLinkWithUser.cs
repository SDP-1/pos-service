using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pos_service.Migrations
{
    /// <inheritdoc />
    public partial class createByUpdateByLinkWithUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_BackupHistories_Users_CreatedBy",
                table: "BackupHistories",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_BackupHistories_Users_UpdatedBy",
                table: "BackupHistories",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_BackupLocations_Users_CreatedBy",
                table: "BackupLocations",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_BackupLocations_Users_UpdatedBy",
                table: "BackupLocations",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Contacts_Users_CreatedBy",
                table: "Contacts",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Contacts_Users_UpdatedBy",
                table: "Contacts",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Users_CreatedBy",
                table: "Customers",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Users_UpdatedBy",
                table: "Customers",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_Users_CreatedBy",
                table: "Inventories",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_Users_UpdatedBy",
                table: "Inventories",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryUnits_Users_CreatedBy",
                table: "InventoryUnits",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryUnits_Users_UpdatedBy",
                table: "InventoryUnits",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemExpiries_Users_CreatedBy",
                table: "ItemExpiries",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemExpiries_Users_UpdatedBy",
                table: "ItemExpiries",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemPrices_Users_CreatedBy",
                table: "ItemPrices",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemPrices_Users_UpdatedBy",
                table: "ItemPrices",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Users_CreatedBy",
                table: "Items",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Users_UpdatedBy",
                table: "Items",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemSuppliers_Users_CreatedBy",
                table: "ItemSuppliers",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemSuppliers_Users_UpdatedBy",
                table: "ItemSuppliers",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_LoanSettlementLogs_Users_CreatedBy",
                table: "LoanSettlementLogs",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_LoanSettlementLogs_Users_UpdatedBy",
                table: "LoanSettlementLogs",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Users_CreatedBy",
                table: "OrderItems",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Users_UpdatedBy",
                table: "OrderItems",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Users_CreatedBy",
                table: "Orders",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Users_UpdatedBy",
                table: "Orders",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_Users_CreatedBy",
                table: "RolePermissions",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_Users_UpdatedBy",
                table: "RolePermissions",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_Users_CreatedBy",
                table: "Roles",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_Users_UpdatedBy",
                table: "Roles",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Settings_Users_CreatedBy",
                table: "Settings",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Settings_Users_UpdatedBy",
                table: "Settings",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Shops_Users_CreatedBy",
                table: "Shops",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Shops_Users_UpdatedBy",
                table: "Shops",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Suppliers_Users_CreatedBy",
                table: "Suppliers",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Suppliers_Users_UpdatedBy",
                table: "Suppliers",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_CreatedBy",
                table: "Users",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_UpdatedBy",
                table: "Users",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Uuid",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BackupHistories_Users_CreatedBy",
                table: "BackupHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_BackupHistories_Users_UpdatedBy",
                table: "BackupHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_BackupLocations_Users_CreatedBy",
                table: "BackupLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_BackupLocations_Users_UpdatedBy",
                table: "BackupLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_Contacts_Users_CreatedBy",
                table: "Contacts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contacts_Users_UpdatedBy",
                table: "Contacts");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Users_CreatedBy",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Users_UpdatedBy",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_Users_CreatedBy",
                table: "Inventories");

            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_Users_UpdatedBy",
                table: "Inventories");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryUnits_Users_CreatedBy",
                table: "InventoryUnits");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryUnits_Users_UpdatedBy",
                table: "InventoryUnits");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemExpiries_Users_CreatedBy",
                table: "ItemExpiries");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemExpiries_Users_UpdatedBy",
                table: "ItemExpiries");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemPrices_Users_CreatedBy",
                table: "ItemPrices");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemPrices_Users_UpdatedBy",
                table: "ItemPrices");

            migrationBuilder.DropForeignKey(
                name: "FK_Items_Users_CreatedBy",
                table: "Items");

            migrationBuilder.DropForeignKey(
                name: "FK_Items_Users_UpdatedBy",
                table: "Items");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemSuppliers_Users_CreatedBy",
                table: "ItemSuppliers");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemSuppliers_Users_UpdatedBy",
                table: "ItemSuppliers");

            migrationBuilder.DropForeignKey(
                name: "FK_LoanSettlementLogs_Users_CreatedBy",
                table: "LoanSettlementLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_LoanSettlementLogs_Users_UpdatedBy",
                table: "LoanSettlementLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Users_CreatedBy",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Users_UpdatedBy",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Users_CreatedBy",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Users_UpdatedBy",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_Users_CreatedBy",
                table: "RolePermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_Users_UpdatedBy",
                table: "RolePermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Roles_Users_CreatedBy",
                table: "Roles");

            migrationBuilder.DropForeignKey(
                name: "FK_Roles_Users_UpdatedBy",
                table: "Roles");

            migrationBuilder.DropForeignKey(
                name: "FK_Settings_Users_CreatedBy",
                table: "Settings");

            migrationBuilder.DropForeignKey(
                name: "FK_Settings_Users_UpdatedBy",
                table: "Settings");

            migrationBuilder.DropForeignKey(
                name: "FK_Shops_Users_CreatedBy",
                table: "Shops");

            migrationBuilder.DropForeignKey(
                name: "FK_Shops_Users_UpdatedBy",
                table: "Shops");

            migrationBuilder.DropForeignKey(
                name: "FK_Suppliers_Users_CreatedBy",
                table: "Suppliers");

            migrationBuilder.DropForeignKey(
                name: "FK_Suppliers_Users_UpdatedBy",
                table: "Suppliers");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_CreatedBy",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_UpdatedBy",
                table: "Users");
            
        }
    }
}
