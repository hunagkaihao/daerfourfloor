using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TuTa.Wms.Migrations
{
    /// <inheritdoc />
    public partial class AddStockFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 添加Grade字段到Stocks表
            migrationBuilder.AddColumn<string>(
                name: "Grade",
                table: "Stocks",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            // 添加ProcessNo字段到Stocks表
            migrationBuilder.AddColumn<string>(
                name: "ProcessNo",
                table: "Stocks",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            // 添加BoxNumber字段到Stocks表的BoxData值对象
            migrationBuilder.AddColumn<string>(
                name: "BoxData_BoxNumber",
                table: "Stocks",
                type: "varchar(30)",
                maxLength: 30,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Grade",
                table: "Stocks");

            migrationBuilder.DropColumn(
                name: "ProcessNo",
                table: "Stocks");

            migrationBuilder.DropColumn(
                name: "BoxData_BoxNumber",
                table: "Stocks");
        }
    }
}
