using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TuTa.Wms.Erp.Dto
{
    /// <summary>
    /// ERP ASN单据DTO
    /// </summary>
    public class ErpAsnDto
    {
        /// <summary>
        /// ASN单号
        /// </summary>
        public string Ccode { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public string Cordercode { get; set; }

        /// <summary>
        /// 供应商简称
        /// </summary>
        public string Cvenabbname { get; set; }

        /// <summary>
        /// 供应商代码
        /// </summary>
        public string Cvencode { get; set; }

        /// <summary>
        /// 仓库代码
        /// </summary>
        public string Cwhcode { get; set; }

        /// <summary>
        /// 仓库名称
        /// </summary>
        public string Cwhname { get; set; }

        /// <summary>
        /// 到货日期
        /// </summary>
        public string Darridate { get; set; }

        /// <summary>
        /// ASN标志
        /// </summary>
        public string Iasnflag { get; set; }

        /// <summary>
        /// 物料编码
        /// </summary>
        public string Cinvcode { get; set; }

        /// <summary>
        /// 物料名称
        /// </summary>
        public string Cinvname { get; set; }

        /// <summary>
        /// 规格型号
        /// </summary>
        public string Cinvstd { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        public string Cinfvm_unit { get; set; }

        /// <summary>
        /// 计划数量
        /// </summary>
        public decimal Ipoquantity { get; set; }

        /// <summary>
        /// 批次
        /// </summary>
        public string Cbatch { get; set; }

        /// <summary>
        /// 到货数量
        /// </summary>
        public decimal Farrqty { get; set; }

        /// <summary>
        /// 已出库数量
        /// </summary>
        public decimal Foutquantity { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public decimal Iquantity { get; set; }

        /// <summary>
        /// 实际数量
        /// </summary>
        public decimal Frealquantity { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Cmemo { get; set; }

        /// <summary>
        /// 制单人
        /// </summary>
        public string Cmaker { get; set; }

        /// <summary>
        /// 日期
        /// </summary>
        public string Ddate { get; set; }

        /// <summary>
        /// 业务类型
        /// </summary>
        public string Cbustype { get; set; }

        /// <summary>
        /// 采购类型代码
        /// </summary>
        public string Cptcode { get; set; }

        /// <summary>
        /// 采购类型名称
        /// </summary>
        public string Cptname { get; set; }

        /// <summary>
        /// 发货日期
        /// </summary>
        public string Dshipdate { get; set; }

        /// <summary>
        /// 部门代码
        /// </summary>
        public string Cdepcode { get; set; }

        /// <summary>
        /// 部门名称
        /// </summary>
        public string Cdepname { get; set; }

        /// <summary>
        /// 人员代码
        /// </summary>
        public string Cpersoncode { get; set; }

        /// <summary>
        /// 人员名称
        /// </summary>
        public string Cpersonname { get; set; }

        /// <summary>
        /// 币种
        /// </summary>
        public string CexchName { get; set; }

        /// <summary>
        /// 主键ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 子表ID
        /// </summary>
        public string Autoid { get; set; }

        /// <summary>
        /// 表头备注
        /// </summary>
        public string Headcmemo { get; set; }

        /// <summary>
        /// 到货日期B
        /// </summary>
        public string Darridateb { get; set; }

        /// <summary>
        /// 制单时间
        /// </summary>
        public string Cmaketime { get; set; }

        /// <summary>
        /// 税率
        /// </summary>
        public decimal Itaxrateb { get; set; }

        /// <summary>
        /// 汇率
        /// </summary>
        public decimal Iexchrate { get; set; }

        /// <summary>
        /// PO明细ID
        /// </summary>
        public long Iposid { get; set; }

        /// <summary>
        /// 是否GSP
        /// </summary>
        public int Bgsp { get; set; }

        /// <summary>
        /// 关闭人
        /// </summary>
        public string Ccloser { get; set; }

        /// <summary>
        /// 自由项2
        /// </summary>
        public string Cfree2 { get; set; }

        /// <summary>
        /// 自由项3
        /// </summary>
        public string Cfree3 { get; set; }

        /// <summary>
        /// 自由项5
        /// </summary>
        public string Cfree5 { get; set; }

        /// <summary>
        /// 物料附加代码
        /// </summary>
        public string Cinvaddcode { get; set; }

        /// <summary>
        /// 未到货数量
        /// </summary>
        public decimal Wdhsl { get; set; }

        /// <summary>
        /// 状态（1=已创建，2=收货中，3=已完成，4=已取消）
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 状态名称
        /// </summary>
        public string StatusName { get; set; }

        /// <summary>
        /// 已经入库数量
        /// </summary>
        public decimal? AlreadyStockInQuantity { get; set; }

        /// <summary>
        /// 待入库数量
        /// </summary>
        public decimal PendingStockInQuantity { get; set; }
    }

    /// <summary>
    /// ASN校验响应DTO
    /// </summary>
    public class ErpAsnValidateResponseDto
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// ASN明细列表
        /// </summary>
        public List<ErpAsnDto> Data { get; set; }
    }

    /// <summary>
    /// ASN保存响应DTO
    /// </summary>
    public class ErpAsnSaveResponseDto
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 保存的ASN明细ID列表
        /// </summary>
        public List<Guid> Data { get; set; }
    }

    /// <summary>
    /// ASN列表响应DTO
    /// </summary>
    public class ErpAsnListResponseDto
    {
        /// <summary>
        /// 数据项
        /// </summary>
        public System.Collections.Generic.List<ErpAsnDto> Items { get; set; }

        /// <summary>
        /// 总数
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// 页码
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// 每页数量
        /// </summary>
        public int PageSize { get; set; }
    }

    /// <summary>
    /// ERP登录请求DTO
    /// </summary>
    public class ErpLoginRequestDto
    {
        /// <summary>
        /// AppKey
        /// </summary>
        public string AppKey { get; set; }

        /// <summary>
        /// AppSecret
        /// </summary>
        public string AppSecret { get; set; }
    }

    /// <summary>
    /// ERP登录响应DTO
    /// </summary>
    public class ErpLoginResponseDto
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Token
        /// </summary>
        public string Token { get; set; }
    }
}
