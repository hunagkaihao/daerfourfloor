using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TuTa.Wms.EntityFrameworkCore;

#nullable disable

namespace TuTa.Wms.Migrations;

/// <summary>新增PDA实物盘点表及按保存时间查询的索引。</summary>
[DbContext(typeof(WmsDbContext))]
[Migration("20261026000000_AddInventoryCounts")]
public class AddInventoryCounts : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "InventoryCounts",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "char(36)", nullable: false, comment: "提交标识，网络重试去重", collation: "ascii_general_ci"),
                Barcode = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false, comment: "物料码扫码原文"),
                MaterialCode = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, comment: "物料编码，条码第1段"),
                ManufacturerCode = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, comment: "产商代号，批次去掉六位日期"),
                Grade = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, comment: "等级，条码第4段"),
                QuantityPerBox = table.Column<int>(type: "int", nullable: false, comment: "整箱数量，单位只/箱"),
                ActualBoxes = table.Column<int>(type: "int", nullable: false, comment: "实际整箱数，单位箱"),
                RemainderQuantity = table.Column<int>(type: "int", nullable: false, comment: "实际零头，单位只"),
                TotalQuantity = table.Column<long>(type: "bigint", nullable: false, comment: "实际总数，整箱数量乘实际箱数加零头，单位只"),
                RecordedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, comment: "服务器本地保存时间，按此日期导出")
            },
            constraints: table => table.PrimaryKey("PK_InventoryCounts", x => x.Id),
            comment: "PDA实物盘点明细")
            .Annotation("MySql:CharSet", "utf8mb4");
        migrationBuilder.CreateIndex(name: "IX_InventoryCounts_RecordedAt", table: "InventoryCounts", column: "RecordedAt");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable("InventoryCounts");
}
