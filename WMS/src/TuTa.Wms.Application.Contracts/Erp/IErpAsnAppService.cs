using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TuTa.Wms.Erp.Dto;
using TuTa.Wms.Erp.IDto;
using Volo.Abp.Application.Services;

namespace TuTa.Wms.Erp
{
    /// <summary>
    /// ERP ASN应用服务接口
    /// </summary>
    public interface IErpAsnAppService : IApplicationService
    {
        /// <summary>
        /// ERP登录（使用默认配置）
        /// </summary>
        /// <returns>登录结果，包含token</returns>
        Task<ErpLoginResponseDto> LoginErpAsync();

        /// <summary>
        /// 通过ASN码获取信息
        /// </summary>
        /// <param name="asnCode">ASN码</param>
        /// <returns>ASN校验响应结果</returns>
        Task<ErpAsnValidateResponseDto> GetAsnInfoAsync(string asnCode);

        /// <summary>
        /// 保存ASN信息到数据库
        /// </summary>
        /// <param name="asnCode">ASN码</param>
        /// <returns>保存结果</returns>
        Task<ErpAsnSaveResponseDto> SaveAsnAsync(string asnCode);

        /// <summary>
        /// 推送ERP收货单
        /// </summary>
        /// <param name="asnCode">ASN码</param>
        /// <returns>推送结果</returns>
        Task<bool> PushErpReceiptAsync(string asnCode);

        /// <summary>
        /// 生成到货单并推送到ERP
        /// </summary>
        /// <param name="input">到货单推送参数</param>
        /// <returns>推送结果</returns>
        Task<PuArrVouchAddResponseDto> PushPuArrVouchAsync(PuArrVouchAddRequestDto input);

        /// <summary>
        /// 生成来料报检单并推送到ERP
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<LLBJDAddResponseDto> PushLLBJDAddAsync(LLBJDAddRequestDto input);
        /// <summary>
        /// 推送采购入库单到U8
        /// </summary>
        Task<CGRKDAddResponseDto> PushCGRKDAddAsync(CGRKDAddRequestDto input);
        /// <summary>
        /// 当同一ASN单号下所有明细均已入库完成时，自动推送到货单
        /// </summary>
        /// <param name="asnCode">ASN单号</param>
        /// <returns>推送结果；未满足推送条件时返回 null</returns>
        Task<PuArrVouchAddResponseDto> TryPushPuArrVouchIfAllLinesCompletedAsync(string asnCode);

        /// <summary>完整ASN到货单生成/失败重试，保留来源单据关联。</summary>
        Task<PuArrVouchAddResponseDto> PushAsnArrivalAsync(string asnCode);
        /// <summary>到货成功后按到货单分别生成来料报检单，失败重试仅处理尚未成功的单据。</summary>
        Task<LLBJDAddResponseDto> PushAsnInspectionReportAsync(string asnCode);

        /// <summary>物料状态变更页按ASN和采购订单确认检验结果，只允许检验中转为合格或不合格。</summary>
        /// <param name="asnCode">送货单对应ASN号。</param>
        /// <param name="orderCode">当前ASN下的采购订单号。</param>
        /// <param name="inspectionStatus">检验结果：2合格、3不合格。</param>
        /// <returns>更新结果及该订单最新ASN明细。</returns>
        Task<ErpAsnValidateResponseDto> SetAsnOrderInspectionStatusAsync(string asnCode, string orderCode, int inspectionStatus);

        /// <summary>从U8查询当前ASN的来料检验单，严格校验来源和审核、合格数量。</summary>
        Task<U8InspectionResultsDto> GetAsnInspectionResultsAsync(string asnCode);
        /// <summary>根据一条ASN对应的已审核合格检验单生成采购入库；失败独立重试。</summary>
        Task<CGRKDAddResponseDto> PushInspectionPurchaseAsync(Guid asnLineId);

        /// <summary>整张发货单实际完成后推送/重试U8销售出库单。</summary>
        Task<CGRKDAddResponseDto> PushSalesOutboundAsync(string deliveryOrderNo);
        /// <summary>补充U8发货来源明细及销售部门，不影响普通库存。</summary>
        Task<CGRKDAddResponseDto> SetSalesSourceAsync(string recordId, long sourceDetailId, string departmentCode);
        /// <summary>仓管员确认发货单所有明细已扫码，之后才允许整单销售推送。</summary>
        Task<CGRKDAddResponseDto> ConfirmDeliveryScanAsync(string deliveryOrderNo);
        /// <summary>读取U8发货单明细及推送状态，供补录和重试。</summary>
        Task<List<ErpOutboundRecordDto>> GetSalesRecordsAsync(string deliveryOrderNo);

        /// <summary>
        /// 获取ASN列表
        /// </summary>
        /// <param name="page">页码</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="asnCode">ASN码</param>
        /// <param name="supplierName">供应商名称</param>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <param name="status">状态（1=已创建，2=收货中，3=已完成，4=已取消）</param>
        /// <returns>ASN列表</returns>
        Task<ErpAsnListResponseDto> GetAsnListAsync(int page, int pageSize, string asnCode = null, string supplierName = null, string startDate = null, string endDate = null, int? status = null);

        /// <summary>
        /// 通过物料编号获取未完成的ASN单据信息
        /// </summary>
        /// <param name="materialCode">物料编号</param>
        /// <returns>ASN明细列表</returns>
        Task<ErpAsnValidateResponseDto> GetIncompleteAsnByMaterialCodeAsync(string materialCode);

        /// <summary>
        /// 从本地ErpAsns表根据ASN码获取数据
        /// </summary>
        /// <param name="asnCode">ASN码</param>
        /// <returns>ASN明细列表</returns>
        Task<ErpAsnValidateResponseDto> GetLocalAsnByCodeAsync(string asnCode);
    }
}
