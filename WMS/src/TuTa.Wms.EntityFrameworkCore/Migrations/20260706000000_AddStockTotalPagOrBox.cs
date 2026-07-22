using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TuTa.Wms.Migrations
{
    /// <inheritdoc />
    public partial class AddStockTotalPagOrBox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalPagOrBox",
                table: "Stocks",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalPagOrBox",
                table: "Stocks");
        }
    }
}
