using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TuTa.Wms.Erp.Entities;
using TuTa.Wms.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace TuTa.Wms.Erp
{
    /// <summary>
    /// ASN默认仓储实现
    /// </summary>
    public class ErpAsnRepository : EfCoreRepository<WmsDbContext, ErpAsn, Guid>, IErpAsnRepository
    {
        public ErpAsnRepository(IDbContextProvider<WmsDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        /// <summary>
        /// 根据ASN单号获取ASN
        /// </summary>
        public async Task<ErpAsn> GetByAsnCodeAsync(string asnCode)
        {
            return await DbContext.Set<ErpAsn>()
                .FirstOrDefaultAsync(x => x.AsnCode == asnCode);
        }

        /// <summary>
        /// 根据ASN单号获取所有明细
        /// </summary>
        public async Task<List<ErpAsn>> GetListByAsnCodeAsync(string asnCode)
        {
            return await DbContext.Set<ErpAsn>()
                .Where(x => x.AsnCode == asnCode)
                .ToListAsync();
        }

        /// <summary>
        /// 检查ASN是否存在
        /// </summary>
        public async Task<bool> ExistsAsync(string asnCode)
        {
            return await DbContext.Set<ErpAsn>()
                .AnyAsync(x => x.AsnCode == asnCode);
        }

        /// <summary>
        /// 获取ASN列表
        /// </summary>
        public async Task<(List<ErpAsn>, int)> GetAsnListAsync(int page, int pageSize, string asnCode = null, string supplierName = null, string startDate = null, string endDate = null, int? status = null)
        {
            var query = DbContext.Set<ErpAsn>().AsQueryable();

            if (!string.IsNullOrEmpty(asnCode))
            {
                query = query.Where(x => x.AsnCode.Contains(asnCode));
            }

            if (!string.IsNullOrEmpty(supplierName))
            {
                query = query.Where(x => x.SupplierName.Contains(supplierName));
            }

            if (status.HasValue && Enum.IsDefined(typeof(AsnStatus), status.Value))
            {
                var asnStatus = (AsnStatus)status.Value;
                query = query.Where(x => x.Status == asnStatus);
            }

            if (!string.IsNullOrEmpty(startDate))
            {
                if (DateTime.TryParse(startDate, out var start))
                {
                    query = query.Where(x => x.BillDate >= start);
                }
            }

            if (!string.IsNullOrEmpty(endDate))
            {
                if (DateTime.TryParse(endDate, out var end))
                {
                    query = query.Where(x => x.BillDate <= end);
                }
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(x => x.BillDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        /// <summary>
        /// 根据物料编号获取未完成的ASN明细
        /// </summary>
        public async Task<List<ErpAsn>> GetIncompleteListByMaterialCodeAsync(string materialCode)
        {
            return await DbContext.Set<ErpAsn>()
                .Where(x =>
                    x.MaterialCode == materialCode &&
                    x.Status != AsnStatus.Completed &&
                    x.Status != AsnStatus.Cancelled &&
                    x.InWarehouseQuantity - (x.AlreadyStockInQuantity ?? 0) > 0)
                .OrderByDescending(x => x.BillDate)
                .ThenByDescending(x => x.CreationTime)
                .ToListAsync();
        }

        /// <summary>
        /// 根据订单号与物料编号获取ASN明细
        /// </summary>
        public async Task<ErpAsn> GetByOrderCodeAndMaterialCodeAsync(string orderCode, string materialCode)
        {
            return await DbContext.Set<ErpAsn>()
                .Where(x =>
                    x.OrderCode == orderCode &&
                    x.MaterialCode == materialCode &&
                    x.Status != AsnStatus.Cancelled)
                .OrderByDescending(x => x.BillDate)
                .ThenByDescending(x => x.CreationTime)
                .FirstOrDefaultAsync();
        }
    }
}
