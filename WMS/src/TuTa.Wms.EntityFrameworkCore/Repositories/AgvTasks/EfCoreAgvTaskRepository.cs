using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using TuTa.Wms.AgvTasks;
using TuTa.Wms.AgvTasks.Aggregaes;
using TuTa.Wms.EntityFrameworkCore;
using TuTa.Wms.AgvTasks.Dtos;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

using Wms.EntityFrameworkCore;

namespace TuTa.Wms.Repositories.AgvTasks
{
    public class EfCoreAgvTaskRepository : EfCoreRepository<WmsDbContext, AgvTask, int>, IAgvTaskRepository
    {
        public EfCoreAgvTaskRepository(IDbContextProvider<WmsDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        public async Task<AgvTask> FindByIdAsync(
            int id,
            bool isTrack = true,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .TrackIf(isTrack)
                .FirstOrDefaultAsync(o => o.Id == id)
                .ConfigureAwait(false);
        }

        public async Task<AgvTask> FindByReqCodeAsync(
            string reqcode,
            bool isTrack = true,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .TrackIf(isTrack)
                .FirstOrDefaultAsync(o => o.ReqCode == reqcode)
                .ConfigureAwait(false);
        }
        public async Task<(List<AgvTask> Items, long TotalCount)> GetPagedListAsync(AgvTaskPagedQueryDto input)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);

            // 构建查询条件 - 在数据库层面进行筛选
            var query = dbSet.AsNoTracking(); // 使用 AsNoTracking 提高性能

            // 应用筛选条件
            if (!string.IsNullOrEmpty(input.ReqCode))
            {
                query = query.Where(x => x.ReqCode.Contains(input.ReqCode));
            }

            if (!string.IsNullOrEmpty(input.ClientCode))
            {
                query = query.Where(x => x.ClientCode.Contains(input.ClientCode));
            }

            if (!string.IsNullOrEmpty(input.TaskTyp))
            {
                query = query.Where(x => x.TaskTyp.Contains(input.TaskTyp));
            }

            if (input.StockTyp.HasValue)
            {
                query = query.Where(x => x.StockTyp == input.StockTyp.Value);
            }

            if (!string.IsNullOrEmpty(input.WbCode))
            {
                query = query.Where(x => x.WbCode.Contains(input.WbCode));
            }

            if (!string.IsNullOrEmpty(input.PodCode))
            {
                query = query.Where(x => x.PodCode.Contains(input.PodCode));
            }

            if (!string.IsNullOrEmpty(input.MaterialLot))
            {
                query = query.Where(x => x.MaterialLot.Contains(input.MaterialLot));
            }

            if (input.AgvTaskStatus.HasValue)
            {
                query = query.Where(x => x.AgvTaskStatus == input.AgvTaskStatus.Value);
            }

            if (!string.IsNullOrEmpty(input.BoxCode))
            {
                query = query.Where(x => x.BoxCode.Contains(input.BoxCode));
            }

            if (!string.IsNullOrEmpty(input.CtnrTyp))
            {
                query = query.Where(x => x.CtnrTyp.Contains(input.CtnrTyp));
            }

            if (!string.IsNullOrEmpty(input.StartPositionCode))
            {
                query = query.Where(x => x.StartPositionCode.Contains(input.StartPositionCode));
            }

            if (!string.IsNullOrEmpty(input.EndPositionCode))
            {
                query = query.Where(x => x.EndPositionCode.Contains(input.EndPositionCode));
            }

            if (!string.IsNullOrEmpty(input.PickListCode))
            {
                query = query.Where(x => x.PickListCode.Contains(input.PickListCode));
            }

            if (!string.IsNullOrEmpty(input.UniqueCode))
            {
                query = query.Where(x => x.UniqueCode.Contains(input.UniqueCode));
            }

            if (!string.IsNullOrEmpty(input.AgvCode))
            {
                query = query.Where(x => x.AgvCode.Contains(input.AgvCode));
            }

            if (!string.IsNullOrEmpty(input.TaskCode))
            {
                query = query.Where(x => x.TaskCode.Contains(input.TaskCode));
            }

            if (input.CreationTimeStart.HasValue)
            {
                query = query.Where(x => x.CreationTime >= input.CreationTimeStart.Value);
            }

            if (input.CreationTimeEnd.HasValue)
            {
                query = query.Where(x => x.CreationTime <= input.CreationTimeEnd.Value);
            }

            // 对于ReqTime的筛选，使用字符串比较
            if (input.ReqTimeStart.HasValue)
            {
                var reqTimeStartStr = input.ReqTimeStart.Value.ToString("yyyy-MM-dd HH:mm:ss");
                query = query.Where(x => x.ReqTime != null && string.Compare(x.ReqTime, reqTimeStartStr) >= 0);
            }

            if (input.ReqTimeEnd.HasValue)
            {
                var reqTimeEndStr = input.ReqTimeEnd.Value.ToString("yyyy-MM-dd HH:mm:ss");
                query = query.Where(x => x.ReqTime != null && string.Compare(x.ReqTime, reqTimeEndStr) <= 0);
            }

            // 先获取总记录数 - 使用单独的查询避免重复计算
            var totalCount = await query.LongCountAsync().ConfigureAwait(false);

            // 应用分页和排序，只查询需要的数据
            var items = await query
                .OrderByDescending(x => x.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync()
                .ConfigureAwait(false);

            return (items, totalCount);
        }
    }
}
