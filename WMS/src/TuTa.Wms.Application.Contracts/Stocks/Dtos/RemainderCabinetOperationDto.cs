using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace TuTa.Wms.Stocks.Dtos
{
    /// <summary>零头柜库存及流水查询条件，文本条件按包含匹配，单页最多200条。</summary>
    public class RemainderCabinetQueryDto : PagedResultRequestDto
    {
        /// <summary>扫描存货编码、物料等级卡或柜位编码；空值表示不按扫码筛选。</summary>
        public string ScanCode { get; set; }
        /// <summary>存货编码片段，可为空。</summary>
        public string MaterialCode { get; set; }
        /// <summary>柜位编码片段，可为空，例如A15。</summary>
        public string CellCode { get; set; }
        /// <summary>等级片段，可为空。</summary>
        public string Grade { get; set; }
        /// <summary>流水日期，支持yyyy、yyyy-MM、yyyy-MM-dd，包含该年、月或日的全部记录。</summary>
        public string DateKeyword { get; set; }
        /// <summary>寄存流水批次号片段；填写后仅查询寄存流水，不参与库存查询或领用匹配。</summary>
        public string BatchCode { get; set; }
    }

    /// <summary>独立寄存请求，按存货、等级、柜位和数量记账；箱码批次仅记录流水，不关联ASN或U8。</summary>
    public class RemainderCabinetDepositDto
    {
        /// <summary>本次操作唯一提交标识，重试保持不变，成功后下一笔才更换。</summary>
        public Guid RequestId { get; set; }
        /// <summary>实际零头柜位编码，例如A15-08。</summary>
        [Required]
        public string CellCode { get; set; }
        /// <summary>扫描识别后的存货编码。</summary>
        [Required]
        public string MaterialCode { get; set; }
        /// <summary>等级，必须与该格固定等级一致。</summary>
        [Required]
        public string Grade { get; set; }
        /// <summary>实际数量，单位同物料，须大于0，最多6位小数。</summary>
        public decimal Quantity { get; set; }
        /// <summary>箱码第2段批次号，仅寄存时校验最多30字符并写入寄存流水；领用忽略该兼容字段。</summary>
        public string BatchCode { get; set; }
    }

    /// <summary>零头柜物料与库位绑定；解绑时MaterialCode为页面读取的原编码，防止误解绑。</summary>
    public class RemainderCabinetBindingDto
    {
        /// <summary>A至L柜的实际格位，服务端统一为大写，例如K01-01。</summary>
        [Required]
        public string CellCode { get; set; }
        /// <summary>绑定的新物料编码或解绑前的原物料编码，必须是已有物料。</summary>
        [Required, StringLength(20)]
        public string MaterialCode { get; set; }
    }

    /// <summary>按存货、等级及柜位领用，忽略批次；数量不能超过该格可领用合计，重试沿用原请求。</summary>
    public class RemainderCabinetWithdrawDto : RemainderCabinetDepositDto { }

    /// <summary>按存货、等级和实际柜位汇总的零头库存，不关联ASN。</summary>
    public class RemainderCabinetBalanceDto
    {
        /// <summary>查询行标识，同存货同等级同柜位仅显示一行。</summary>
        public string Id => CellCode + ":" + MaterialCode + ":" + Grade;
        /// <summary>存货编码。</summary>
        public string MaterialCode { get; set; }
        /// <summary>存货名称，来自已有物料主数据。</summary>
        public string MaterialName { get; set; }
        /// <summary>实际柜位。</summary>
        public string CellCode { get; set; }
        /// <summary>物料等级。</summary>
        public string Grade { get; set; }
        /// <summary>当前在柜总数量，包含暂不可用数量。</summary>
        public decimal Quantity { get; set; }
        /// <summary>当前可领用数量，排除冻结、锁定或被其他业务占用的库存。</summary>
        public decimal AvailableQuantity { get; set; }
        /// <summary>库存计量单位。</summary>
        public string Unit { get; set; }
    }

    /// <summary>零头柜寄存或领用流水，来自既有入库、出库流水表。</summary>
    public class RemainderCabinetHistoryDto
    {
        /// <summary>既有流水表主键，结合Operation区分入库与出库记录。</summary>
        public int HistoryId { get; set; }
        /// <summary>前端唯一行标识。</summary>
        public string Id => Operation + ":" + HistoryId;
        /// <summary>操作类型：寄存或领用。</summary>
        public string Operation { get; set; }
        /// <summary>实际操作时间。</summary>
        public DateTime OperationTime { get; set; }
        /// <summary>存货编码。</summary>
        public string MaterialCode { get; set; }
        /// <summary>存货名称。</summary>
        public string MaterialName { get; set; }
        /// <summary>实际货位。</summary>
        public string CellCode { get; set; }
        /// <summary>操作时记录的等级；解绑或更换物料不改写旧流水，未记录时为空。</summary>
        public string Grade { get; set; }
        /// <summary>寄存扫描箱码的追溯批次；领用及历史未采集批次的寄存流水为空。</summary>
        public string BatchCode { get; set; }
        /// <summary>本次寄存或领用数量，始终为正数，由Operation区分方向。</summary>
        public decimal Quantity { get; set; }
        /// <summary>库存计量单位。</summary>
        public string Unit { get; set; }
        /// <summary>记录的操作人员。</summary>
        public string OperatorName { get; set; }
    }
}
