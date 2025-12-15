using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pos_service.Migrations
{
    /// <inheritdoc />
    public partial class itemSupplayer_table_add : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemSupplier");

            migrationBuilder.CreateTable(
                name: "ItemSuppliers",
                columns: table => new
                {
                    SuppliersId = table.Column<int>(type: "int", nullable: false),
                    ItemsId = table.Column<int>(type: "int", nullable: false),
                    ItemsSubId = table.Column<int>(type: "int", nullable: false),
                    Uuid = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemSuppliers", x => new { x.SuppliersId, x.ItemsId, x.ItemsSubId });
                    table.UniqueConstraint("AK_ItemSuppliers_Uuid", x => x.Uuid);
                    table.ForeignKey(
                        name: "FK_ItemSuppliers_Items_ItemsId_ItemsSubId",
                        columns: x => new { x.ItemsId, x.ItemsSubId },
                        principalTable: "Items",
                        principalColumns: new[] { "Id", "SubId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemSuppliers_Suppliers_SuppliersId",
                        column: x => x.SuppliersId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ItemSuppliers_ItemsId_ItemsSubId",
                table: "ItemSuppliers",
                columns: new[] { "ItemsId", "ItemsSubId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemSuppliers");

            migrationBuilder.CreateTable(
                name: "ItemSupplier",
                columns: table => new
                {
                    SuppliersId = table.Column<int>(type: "int", nullable: false),
                    ItemsId = table.Column<int>(type: "int", nullable: false),
                    ItemsSubId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemSupplier", x => new { x.SuppliersId, x.ItemsId, x.ItemsSubId });
                    table.ForeignKey(
                        name: "FK_ItemSupplier_Items_ItemsId_ItemsSubId",
                        columns: x => new { x.ItemsId, x.ItemsSubId },
                        principalTable: "Items",
                        principalColumns: new[] { "Id", "SubId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemSupplier_Suppliers_SuppliersId",
                        column: x => x.SuppliersId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ItemSupplier_ItemsId_ItemsSubId",
                table: "ItemSupplier",
                columns: new[] { "ItemsId", "ItemsSubId" });
        }
    }
}
