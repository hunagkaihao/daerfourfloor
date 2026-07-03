using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TuTa.Wms.EntityFrameworkCore;
using TuTa.Wms.Moves;
using TuTa.Wms.Moves.Aggregates;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Wms.EntityFrameworkCore;

namespace TuTa.Wms.Repositories.Moves
{
    public class EfCoreMoveRepository : EfCoreRepository<WmsDbContext, Move, Guid>, IMoveRepository
    {
        public EfCoreMoveRepository(IDbContextProvider<WmsDbContext> dbContextProvider) : base(dbContextProvider)
        {

        }
        public async Task<Move> FindByMoveCodeAsync(
            string code,
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .TrackIf(isTrack)
                .FirstOrDefaultAsync(o => o.MoveCode == code, cancellationToken)
                .ConfigureAwait(false);
        }
        public async Task<Move> FindByCheckNoEnableAsync(
            string checkNo,
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .TrackIf(isTrack)
                .FirstOrDefaultAsync(o => o.CheckNo == checkNo && o.CountToMove != o.MoveCount, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
