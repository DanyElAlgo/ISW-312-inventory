using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sales.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketTenantColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "company_cen",
                schema: "sales",
                table: "order_ticket",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "warehouse_cen",
                schema: "sales",
                table: "order_ticket",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "company_cen",
                schema: "sales",
                table: "order_ticket");

            migrationBuilder.DropColumn(
                name: "warehouse_cen",
                schema: "sales",
                table: "order_ticket");
        }
    }
}
