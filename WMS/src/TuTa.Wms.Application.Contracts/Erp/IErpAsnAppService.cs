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
        /// <summary>按当前ASN中的指定采购订单生成完整到货单，供明细入口独立推送和重试。</summary>
        Task<PuArrVouchAddResponseDto> PushAsnOrderArrivalAsync(string asnCode, string orderCode);
        /// <summary>到货成功后按到货单分别生成来料报检单，失败重试仅处理尚未成功的单据。</summary>
        Task<LLBJDAddResponseDto> PushAsnInspectionReportAsync(string asnCode);
        /// <summary>按指定采购订单对应的U8到货单报检，不依赖其他采购订单的到货推送结果。</summary>
        Task<LLBJDAddResponseDto> PushAsnOrderInspectionReportAsync(string asnCode, string orderCode);

        /// <summary>物料状态变更页按ASN和采购订单确认检验结果，只允许检验中转为合格或不合格。</summary>
        /// <param name="asnCode">送货单对应ASN号。</param>
        /// <param name="orderCode">当前ASN下的采购订单号。</param>
        /// <param name="inspectionStatus">检验结果：2合格、3不合格。</param>
        /// <returns>更新结果及该订单最新ASN明细。</returns>
        Task<ErpAsnValidateResponseDto> SetAsnOrderInspectionStatusAsync(string asnCode, string orderCode, int inspectionStatus);

        /// <summary>从U8查询当前ASN的来料检验单，严格校验来源和审核、合格数量。</summary>
        Task<U8InspectionResultsDto> GetAsnInspectionResultsAsync(string asnCode);
        /// <summary>分页查询U8来料检验单，可选按物料、来源到货单及制单开始日期筛选，包含开始日期当天。</summary>
        /// <param name="input">全部筛选留空时按U8默认条件查询；页码从1开始，不修改本地检验结论。</param>
        /// <returns>当前页匹配单据及是否有后续页，不将存在单据视为合格。</returns>
        Task<U8MaterialInspectionResultDto> GetMaterialInspectionResultsAsync(U8MaterialInspectionQueryDto input);

        /// <summary>重新核对所选U8检验单已审核后，只确认该记录对应ASN明细的合格或不合格结果。</summary>
        /// <param name="input">检验单身份、原查询分页条件及人工检验结论。</param>
        /// <returns>确认结果及该检验记录对应的最新ASN明细。</returns>
        Task<ErpAsnValidateResponseDto> ConfirmMaterialInspectionAsync(U8MaterialInspectionConfirmDto input);
        /// <summary>按指定ASN明细本地已确认的合格检验单来源逐单生成采购入库，不再查询U8检验单；该明细账实一致即可，不等待整张ASN，失败独立重试。</summary>
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
        /// <param name="supplierCode">供应商编码，去除首尾空格后按包含匹配；空值不筛选。</param>
        /// <returns>ASN列表</returns>
        Task<ErpAsnListResponseDto> GetAsnListAsync(int page, int pageSize, string asnCode = null, string supplierName = null, string startDate = null, string endDate = null, int? status = null, string supplierCode = null);

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
