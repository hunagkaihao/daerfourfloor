using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TuTa.Wms.Erp.Entities;
using Volo.Abp.Domain.Repositories;

namespace TuTa.Wms.Erp
{
    /// <summary>
    /// ASN仓储接口
    /// </summary>
    public interface IErpAsnRepository : IRepository<ErpAsn, Guid>
    {
        /// <summary>
        /// 根据ASN单号获取ASN
        /// </summary>
        Task<ErpAsn> GetByAsnCodeAsync(string asnCode);

        /// <summary>
        /// 根据ASN单号获取所有明细
        /// </summary>
        Task<List<ErpAsn>> GetListByAsnCodeAsync(string asnCode);

        /// <summary>
        /// 检查ASN是否存在
        /// </summary>
        Task<bool> ExistsAsync(string asnCode);

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
        /// <returns>ASN列表和总数</returns>
        Task<(List<ErpAsn>, int)> GetAsnListAsync(int page, int pageSize, string asnCode = null, string supplierName = null, string startDate = null, string endDate = null, int? status = null);

        /// <summary>
        /// 根据物料编号获取未完成的ASN明细
        /// </summary>
        Task<List<ErpAsn>> GetIncompleteListByMaterialCodeAsync(string materialCode);

        /// <summary>
        /// 根据订单号与物料编号获取ASN明细
        /// </summary>
        Task<ErpAsn> GetByOrderCodeAndMaterialCodeAsync(string orderCode, string materialCode);
    }
}
