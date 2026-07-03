using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TuTa.Wms.Migrations
{
    /// <inheritdoc />
    public partial class UpdateErpAsnFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsClosed",
                table: "ErpAsns",
                newName: "IsGsp");

            migrationBuilder.AddColumn<DateTime>(
                name: "ArrivalDateB",
                table: "ErpAsns",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Closer",
                table: "ErpAsns",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Free2",
                table: "ErpAsns",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Free3",
                table: "ErpAsns",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Free5",
                table: "ErpAsns",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Headcmemo",
                table: "ErpAsns",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "MakeTime",
                table: "ErpAsns",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "MaterialAddCode",
                table: "ErpAsns",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "NotArrivedQuantity",
                table: "ErpAsns",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArrivalDateB",
                table: "ErpAsns");

            migrationBuilder.DropColumn(
                name: "Closer",
                table: "ErpAsns");

            migrationBuilder.DropColumn(
                name: "Free2",
                table: "ErpAsns");

            migrationBuilder.DropColumn(
                name: "Free3",
                table: "ErpAsns");

            migrationBuilder.DropColumn(
                name: "Free5",
                table: "ErpAsns");

            migrationBuilder.DropColumn(
                name: "Headcmemo",
                table: "ErpAsns");

            migrationBuilder.DropColumn(
                name: "MakeTime",
                table: "ErpAsns");

            migrationBuilder.DropColumn(
                name: "MaterialAddCode",
                table: "ErpAsns");

            migrationBuilder.DropColumn(
                name: "NotArrivedQuantity",
                table: "ErpAsns");

            migrationBuilder.RenameColumn(
                name: "IsGsp",
                table: "ErpAsns",
                newName: "IsClosed");
        }
    }
}
