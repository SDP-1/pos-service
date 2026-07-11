using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pos_service.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSqlTemplateStatusAndPreviewEnabled : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE SqlTemplates SET IsActive = 0 WHERE Status = 'Inactive' OR Status = 'Disabled';");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "SqlTemplates");

            migrationBuilder.DropColumn(
                name: "PreviewEnabled",
                table: "ReportTemplates");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "SqlTemplates",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "PreviewEnabled",
                table: "ReportTemplates",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }
    }
}
