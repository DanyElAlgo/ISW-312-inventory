using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Sales.API.Migrations
{
    /// <inheritdoc />
    public partial class ContractCompliance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "sales");

            migrationBuilder.CreateTable(
                name: "customer",
                schema: "sales",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("customer_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "global_tax_config",
                schema: "sales",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tax_rate = table.Column<decimal>(type: "numeric(7,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("global_tax_config_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "order_status",
                schema: "sales",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("order_status_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payment_type",
                schema: "sales",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("payment_type_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "station_type",
                schema: "sales",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("station_type_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "waiter",
                schema: "sales",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("waiter_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "order_ticket",
                schema: "sales",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    customer_id = table.Column<int>(type: "integer", nullable: true),
                    status_id = table.Column<int>(type: "integer", nullable: true),
                    tax_rate_snapshot = table.Column<decimal>(type: "numeric(7,4)", nullable: true, defaultValue: 0m),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    daily_number = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    cancellation_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("order_ticket_pkey", x => x.id);
                    table.ForeignKey(
                        name: "order_ticket_customer_id_fkey",
                        column: x => x.customer_id,
                        principalSchema: "sales",
                        principalTable: "customer",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "order_ticket_status_id_fkey",
                        column: x => x.status_id,
                        principalSchema: "sales",
                        principalTable: "order_status",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "station",
                schema: "sales",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    type_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("station_pkey", x => x.id);
                    table.ForeignKey(
                        name: "station_type_id_fkey",
                        column: x => x.type_id,
                        principalSchema: "sales",
                        principalTable: "station_type",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "order_command",
                schema: "sales",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_id = table.Column<int>(type: "integer", nullable: true),
                    waiter_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("order_command_pkey", x => x.id);
                    table.ForeignKey(
                        name: "order_command_order_id_fkey",
                        column: x => x.order_id,
                        principalSchema: "sales",
                        principalTable: "order_ticket",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "order_command_waiter_id_fkey",
                        column: x => x.waiter_id,
                        principalSchema: "sales",
                        principalTable: "waiter",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "order_item",
                schema: "sales",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    qty = table.Column<double>(type: "double precision", nullable: true),
                    additional_note = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    order_id = table.Column<int>(type: "integer", nullable: true),
                    product_id = table.Column<int>(type: "integer", nullable: true),
                    product_cen = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    product_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    unit_price = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    status_id = table.Column<int>(type: "integer", nullable: true),
                    sent_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    resend_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("order_item_pkey", x => x.id);
                    table.ForeignKey(
                        name: "order_item_order_id_fkey",
                        column: x => x.order_id,
                        principalSchema: "sales",
                        principalTable: "order_ticket",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "order_item_status_id_fkey",
                        column: x => x.status_id,
                        principalSchema: "sales",
                        principalTable: "order_status",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "payment",
                schema: "sales",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_id = table.Column<int>(type: "integer", nullable: true),
                    payment_type_id = table.Column<int>(type: "integer", nullable: true),
                    paid_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("payment_pkey", x => x.id);
                    table.ForeignKey(
                        name: "payment_order_id_fkey",
                        column: x => x.order_id,
                        principalSchema: "sales",
                        principalTable: "order_ticket",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "payment_payment_type_id_fkey",
                        column: x => x.payment_type_id,
                        principalSchema: "sales",
                        principalTable: "payment_type",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "command_item",
                schema: "sales",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_item_id = table.Column<int>(type: "integer", nullable: true),
                    command_id = table.Column<int>(type: "integer", nullable: true),
                    station_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("command_item_pkey", x => x.id);
                    table.ForeignKey(
                        name: "command_item_order_item_id_fkey",
                        column: x => x.order_item_id,
                        principalSchema: "sales",
                        principalTable: "order_item",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "command_item_station_id_fkey",
                        column: x => x.station_id,
                        principalSchema: "sales",
                        principalTable: "station",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_command_item_order_item_id",
                schema: "sales",
                table: "command_item",
                column: "order_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_command_item_station_id",
                schema: "sales",
                table: "command_item",
                column: "station_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_command_order_id",
                schema: "sales",
                table: "order_command",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_command_waiter_id",
                schema: "sales",
                table: "order_command",
                column: "waiter_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_item_order_id",
                schema: "sales",
                table: "order_item",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_item_status_id",
                schema: "sales",
                table: "order_item",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_ticket_customer_id",
                schema: "sales",
                table: "order_ticket",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_ticket_status_id",
                schema: "sales",
                table: "order_ticket",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "IX_payment_order_id",
                schema: "sales",
                table: "payment",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_payment_payment_type_id",
                schema: "sales",
                table: "payment",
                column: "payment_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_station_type_id",
                schema: "sales",
                table: "station",
                column: "type_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "command_item",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "global_tax_config",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "order_command",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "payment",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "order_item",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "station",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "waiter",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "payment_type",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "order_ticket",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "station_type",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "customer",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "order_status",
                schema: "sales");
        }
    }
}
