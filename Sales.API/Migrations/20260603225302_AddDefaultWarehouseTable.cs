using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sales.API.Migrations
{
    /// <inheritdoc />
    public partial class AddDefaultWarehouseTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "default_warehouse",
                schema: "sales",
                columns: table => new
                {
                    company_cen = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    warehouse_cen = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("default_warehouse_pkey", x => x.company_cen);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "default_warehouse",
                schema: "sales");
        }
    }
}
