using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TuTa.Wms.Migrations
{
    /// <inheritdoc />
    public partial class AddErpOutboundActualOutboundQuantity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ActualOutboundQuantity",
                table: "erpoutbound",
                type: "decimal(18,4)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActualOutboundQuantity",
                table: "erpoutbound");
        }
    }
}
