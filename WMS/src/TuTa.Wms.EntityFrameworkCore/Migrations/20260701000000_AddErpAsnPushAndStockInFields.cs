using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TuTa.Wms.Migrations
{
    /// <inheritdoc />
    public partial class AddErpAsnPushAndStockInFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPushedToErp",
                table: "ErpAsns",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PushTime",
                table: "ErpAsns",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastStockInTime",
                table: "ErpAsns",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "StockInQuantity",
                table: "ErpAsns",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPushedToErp",
                table: "ErpAsns");

            migrationBuilder.DropColumn(
                name: "PushTime",
                table: "ErpAsns");

            migrationBuilder.DropColumn(
                name: "LastStockInTime",
                table: "ErpAsns");

            migrationBuilder.DropColumn(
                name: "StockInQuantity",
                table: "ErpAsns");
        }
    }
}
