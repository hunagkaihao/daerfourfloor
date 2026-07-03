using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TuTa.Wms.Migrations
{
    /// <inheritdoc />
    public partial class AddCellLaneFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LaneToColumn",
                table: "Cells",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "LanePosition",
                table: "Cells",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LaneToColumn",
                table: "Cells");

            migrationBuilder.DropColumn(
                name: "LanePosition",
                table: "Cells");
        }
    }
}
