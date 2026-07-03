using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TuTa.Wms.Moves.Aggregates;
using Volo.Abp.Domain.Repositories;

namespace TuTa.Wms.Moves
{
    public interface IMoveRepository : IRepository<Move,Guid>
    {
        Task<Move> FindByMoveCodeAsync(
            string code,
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default);

        Task<Move> FindByCheckNoEnableAsync(
            string checkNo,
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default);

    }
}
