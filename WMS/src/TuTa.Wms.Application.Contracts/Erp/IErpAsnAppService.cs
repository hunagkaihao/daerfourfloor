using System;
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
        /// 当同一ASN单号下所有明细均已入库完成时，自动推送到货单
        /// </summary>
        /// <param name="asnCode">ASN单号</param>
        /// <returns>推送结果；未满足推送条件时返回 null</returns>
        Task<PuArrVouchAddResponseDto> TryPushPuArrVouchIfAllLinesCompletedAsync(string asnCode);

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
