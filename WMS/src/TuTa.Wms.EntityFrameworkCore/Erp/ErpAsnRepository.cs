using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
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
            return await GetNullSafeInspectionQuery()
                .Where(x => x.AsnCode == asnCode)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<List<ErpAsn>> GetInspectionSourcesAsync(List<string> materialCodes)
        {
            if (materialCodes == null || materialCodes.Count == 0) return new List<ErpAsn>();
            return await GetNullSafeInspectionQuery().AsNoTracking()
                .Where(line => materialCodes.Contains(line.MaterialCode) && line.Status != AsnStatus.Cancelled)
                .ToListAsync();
        }

        /// <summary>在SQL侧将旧布尔列的NULL转成false，避免EF物化实体时调用GetBoolean读取DBNull。</summary>
        /// <returns>可继续组合筛选的ASN查询；不写回数据，不将缺失推送标志解释为成功。</returns>
        private IQueryable<ErpAsn> GetNullSafeInspectionQuery()
        {
            var context = DbContext;
            var entityType = context.Model.FindEntityType(typeof(ErpAsn));
            var table = StoreObjectIdentifier.Table(entityType.GetTableName(), entityType.GetSchema());
            var sql = context.GetService<ISqlGenerationHelper>();
            // 列名、表名及false字面量来自EF映射，业务筛选仍交由LINQ参数化。
            var columns = entityType.GetProperties().Select(property =>
            {
                var column = sql.DelimitIdentifier(property.GetColumnName(table));
                return property.ClrType == typeof(bool)
                    ? $"COALESCE({column}, {property.GetRelationalTypeMapping().GenerateSqlLiteral(false)}) AS {column}"
                    : column;
            });
            var tableName = sql.DelimitIdentifier(table.Name, table.Schema);
            return context.Set<ErpAsn>().FromSqlRaw($"SELECT {string.Join(", ", columns)} FROM {tableName}");
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
        /// <param name="page">页码，从1开始。</param>
        /// <param name="pageSize">每页ASN单据数，限制为1至200。</param>
        /// <param name="asnCode">可选ASN码，按包含匹配。</param>
        /// <param name="supplierName">可选供应商名称，按包含匹配。</param>
        /// <param name="startDate">可选制单开始日期。</param>
        /// <param name="endDate">可选制单结束日期，包含当日。</param>
        /// <param name="status">可选组盘状态，1至4分别为已创建、收货中、已完成、已取消。</param>
        /// <param name="supplierCode">供应商编码，去除首尾空格后按包含匹配；空值不筛选。</param>
        /// <returns>分页选中的ASN完整明细和匹配的ASN单据总数。</returns>
        public async Task<(List<ErpAsn>, int)> GetAsnListAsync(int page, int pageSize, string asnCode = null, string supplierName = null, string startDate = null, string endDate = null, int? status = null, string supplierCode = null)
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

            if (!string.IsNullOrWhiteSpace(supplierCode))
            {
                var code = supplierCode.Trim();
                query = query.Where(x => x.SupplierCode.Contains(code));
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
                    var exclusiveEnd = end.Date.AddDays(1);
                    query = query.Where(x => x.BillDate < exclusiveEnd);
                }
            }

            // 先按ASN单号分页，再读取完整明细，避免状态筛选或分页截断整单。
            page = Math.Max(1, page);
            pageSize = Math.Max(1, Math.Min(200, pageSize));
            var groups = query.GroupBy(x => x.AsnCode)
                .Select(g => new { Code = g.Key, BillDate = g.Max(x => x.BillDate) });
            var total = await groups.CountAsync();
            var codes = await groups.OrderByDescending(x => x.BillDate).ThenBy(x => x.Code)
                .Skip((page - 1) * pageSize).Take(pageSize).Select(x => x.Code).ToListAsync();
            var allItems = await DbContext.Set<ErpAsn>().Where(x => codes.Contains(x.AsnCode))
                .OrderBy(x => x.OrderCode).ThenBy(x => x.MaterialCode).ThenBy(x => x.Id).ToListAsync();
            var items = codes.SelectMany(code => allItems.Where(x => x.AsnCode == code)).ToList();

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
