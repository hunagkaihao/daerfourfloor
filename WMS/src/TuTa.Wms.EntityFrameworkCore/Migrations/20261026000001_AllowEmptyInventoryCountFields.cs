using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TuTa.Wms.EntityFrameworkCore;

#nullable disable

namespace TuTa.Wms.Migrations;

/// <summary>盘点仅物料码必填，其余业务字段允许为空，兼容已执行的初始建表迁移。</summary>
[DbContext(typeof(WmsDbContext))]
[Migration("20261026000001_AllowEmptyInventoryCountFields")]
public class AllowEmptyInventoryCountFields : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(name: "ManufacturerCode", table: "InventoryCounts", type: "varchar(50)", maxLength: 50,
            nullable: true, comment: "产商代号，批次去掉六位日期，可为空", oldClrType: typeof(string), oldType: "varchar(50)", oldMaxLength: 50, oldComment: "产商代号，批次去掉六位日期");
        migrationBuilder.AlterColumn<string>(name: "Grade", table: "InventoryCounts", type: "varchar(20)", maxLength: 20,
            nullable: true, comment: "等级，条码第4段，可为空", oldClrType: typeof(string), oldType: "varchar(20)", oldMaxLength: 20, oldComment: "等级，条码第4段");
        migrationBuilder.AlterColumn<int>(name: "QuantityPerBox", table: "InventoryCounts", type: "int", nullable: true,
            comment: "整箱数量，单位只/箱，未填为空", oldClrType: typeof(int), oldType: "int", oldComment: "整箱数量，单位只/箱");
        migrationBuilder.AlterColumn<int>(name: "ActualBoxes", table: "InventoryCounts", type: "int", nullable: true,
            comment: "实际整箱数，单位箱，未填为空", oldClrType: typeof(int), oldType: "int", oldComment: "实际整箱数，单位箱");
        migrationBuilder.AlterColumn<int>(name: "RemainderQuantity", table: "InventoryCounts", type: "int", nullable: true,
            comment: "实际零头，单位只，未填为空", oldClrType: typeof(int), oldType: "int", oldComment: "实际零头，单位只");
        migrationBuilder.AlterColumn<long>(name: "TotalQuantity", table: "InventoryCounts", type: "bigint", nullable: true,
            comment: "实际总数，单位只，三项数量未齐时为空", oldClrType: typeof(long), oldType: "bigint", oldComment: "实际总数，整箱数量乘实际箱数加零头，单位只");
    }

    /// <summary>恢复初始必填约束；回退前应先补齐已有空字段。</summary>
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(name: "ManufacturerCode", table: "InventoryCounts", type: "varchar(50)", maxLength: 50,
            nullable: false, comment: "产商代号，批次去掉六位日期", oldClrType: typeof(string), oldType: "varchar(50)", oldMaxLength: 50, oldNullable: true, oldComment: "产商代号，批次去掉六位日期，可为空");
        migrationBuilder.AlterColumn<string>(name: "Grade", table: "InventoryCounts", type: "varchar(20)", maxLength: 20,
            nullable: false, comment: "等级，条码第4段", oldClrType: typeof(string), oldType: "varchar(20)", oldMaxLength: 20, oldNullable: true, oldComment: "等级，条码第4段，可为空");
        migrationBuilder.AlterColumn<int>(name: "QuantityPerBox", table: "InventoryCounts", type: "int", nullable: false,
            comment: "整箱数量，单位只/箱", oldClrType: typeof(int), oldType: "int", oldNullable: true, oldComment: "整箱数量，单位只/箱，未填为空");
        migrationBuilder.AlterColumn<int>(name: "ActualBoxes", table: "InventoryCounts", type: "int", nullable: false,
            comment: "实际整箱数，单位箱", oldClrType: typeof(int), oldType: "int", oldNullable: true, oldComment: "实际整箱数，单位箱，未填为空");
        migrationBuilder.AlterColumn<int>(name: "RemainderQuantity", table: "InventoryCounts", type: "int", nullable: false,
            comment: "实际零头，单位只", oldClrType: typeof(int), oldType: "int", oldNullable: true, oldComment: "实际零头，单位只，未填为空");
        migrationBuilder.AlterColumn<long>(name: "TotalQuantity", table: "InventoryCounts", type: "bigint", nullable: false,
            comment: "实际总数，整箱数量乘实际箱数加零头，单位只", oldClrType: typeof(long), oldType: "bigint", oldNullable: true, oldComment: "实际总数，单位只，三项数量未齐时为空");
    }
}
