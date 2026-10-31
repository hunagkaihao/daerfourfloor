using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TuTa.Wms.EntityFrameworkCore;

#nullable disable

namespace TuTa.Wms.Migrations
{
    /// <summary>只新增U8单据关联和推送字段，普通库存字段及历史数据保持不变。</summary>
    [DbContext(typeof(WmsDbContext))]
    [Migration("20261014000000_AddU8WorkflowTracking")]
    public partial class AddU8WorkflowTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(name: "IsDeliveryScanCompleted", table: "erpoutbound", type: "tinyint(1)", nullable: false, defaultValue: false, comment: "整张U8发货单已确认全部扫码");
            migrationBuilder.AddColumn<long>(name: "ArrivalPushUid", table: "ErpAsns", type: "bigint", nullable: true, comment: "U8到货单推送UID，失败重试沿用");
            migrationBuilder.AddColumn<long>(name: "ArrivalVoucherId", table: "ErpAsns", type: "bigint", nullable: true, comment: "U8到货单主表ID");
            migrationBuilder.AddColumn<string>(name: "ArrivalVoucherCode", table: "ErpAsns", type: "varchar(50)", nullable: true, maxLength: 50, comment: "U8到货单号");
            migrationBuilder.AddColumn<long>(name: "ArrivalDetailId", table: "ErpAsns", type: "bigint", nullable: true, comment: "U8到货单明细autoid");
            migrationBuilder.AddColumn<string>(name: "ArrivalPushError", table: "ErpAsns", type: "varchar(2000)", nullable: true, maxLength: 2000, comment: "U8到货单最近失败原因");
            migrationBuilder.AddColumn<DateTime>(name: "ArrivalPushAttemptTime", table: "ErpAsns", type: "datetime(6)", nullable: true, comment: "U8到货单最近推送尝试时间");
            migrationBuilder.AddColumn<long>(name: "InspectionPushUid", table: "ErpAsns", type: "bigint", nullable: true, comment: "U8来料报检单推送UID");
            migrationBuilder.AddColumn<bool>(name: "IsInspectionReportPushed", table: "ErpAsns", type: "tinyint(1)", nullable: false, defaultValue: false, comment: "U8来料报检单是否成功推送");
            migrationBuilder.AddColumn<string>(name: "InspectionPushError", table: "ErpAsns", type: "varchar(2000)", nullable: true, maxLength: 2000, comment: "U8来料报检单最近失败原因");
            migrationBuilder.AddColumn<Guid>(name: "U8AsnLineId", table: "Stocks", type: "char(36)", nullable: true, comment: "U8关联ASN明细ID，普通库存为空");
            migrationBuilder.AddColumn<string>(name: "U8OutboundRecordId", table: "Stocks", type: "varchar(36)", nullable: true, maxLength: 36, comment: "U8发货记录ID，普通库存为空");
            migrationBuilder.AddColumn<long>(name: "SourceDetailId", table: "erpoutbound", type: "bigint", nullable: true, comment: "U8发货来源明细ID，不能用主表ID替代");
            migrationBuilder.AddColumn<string>(name: "SalesDepartmentCode", table: "erpoutbound", type: "varchar(20)", nullable: true, maxLength: 20, comment: "U8销售部门编码");
            migrationBuilder.AddColumn<string>(name: "ActualOutboundDetailsJson", table: "erpoutbound", type: "longtext", nullable: true, comment: "实际出库批次及数量JSON数组");
            migrationBuilder.AddColumn<long>(name: "SalesPushTaskId", table: "erpoutbound", type: "bigint", nullable: true, comment: "U8销售推送UID，失败重试沿用");
            migrationBuilder.AddColumn<bool>(name: "IsSalesPushed", table: "erpoutbound", type: "tinyint(1)", nullable: false, defaultValue: false, comment: "U8销售出库单是否成功推送");
            migrationBuilder.AddColumn<DateTime>(name: "SalesPushTime", table: "erpoutbound", type: "datetime(6)", nullable: true, comment: "U8销售出库推送成功时间");
            migrationBuilder.AddColumn<string>(name: "SalesPushError", table: "erpoutbound", type: "varchar(2000)", nullable: true, maxLength: 2000, comment: "U8销售出库最近失败原因");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "IsDeliveryScanCompleted", table: "erpoutbound");
            migrationBuilder.DropColumn(name: "SalesPushError", table: "erpoutbound");
            migrationBuilder.DropColumn(name: "SalesPushTime", table: "erpoutbound");
            migrationBuilder.DropColumn(name: "IsSalesPushed", table: "erpoutbound");
            migrationBuilder.DropColumn(name: "SalesPushTaskId", table: "erpoutbound");
            migrationBuilder.DropColumn(name: "ActualOutboundDetailsJson", table: "erpoutbound");
            migrationBuilder.DropColumn(name: "SalesDepartmentCode", table: "erpoutbound");
            migrationBuilder.DropColumn(name: "SourceDetailId", table: "erpoutbound");
            migrationBuilder.DropColumn(name: "U8OutboundRecordId", table: "Stocks");
            migrationBuilder.DropColumn(name: "U8AsnLineId", table: "Stocks");
            migrationBuilder.DropColumn(name: "InspectionPushError", table: "ErpAsns");
            migrationBuilder.DropColumn(name: "IsInspectionReportPushed", table: "ErpAsns");
            migrationBuilder.DropColumn(name: "InspectionPushUid", table: "ErpAsns");
            migrationBuilder.DropColumn(name: "ArrivalPushAttemptTime", table: "ErpAsns");
            migrationBuilder.DropColumn(name: "ArrivalPushError", table: "ErpAsns");
            migrationBuilder.DropColumn(name: "ArrivalDetailId", table: "ErpAsns");
            migrationBuilder.DropColumn(name: "ArrivalVoucherCode", table: "ErpAsns");
            migrationBuilder.DropColumn(name: "ArrivalVoucherId", table: "ErpAsns");
            migrationBuilder.DropColumn(name: "ArrivalPushUid", table: "ErpAsns");
        }
    }
}
