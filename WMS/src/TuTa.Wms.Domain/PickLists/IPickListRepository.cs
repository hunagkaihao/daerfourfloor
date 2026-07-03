using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TuTa.Wms.Domain;
using TuTa.Wms.PickLists.Aggregates;
using Volo.Abp.Domain.Repositories;

namespace TuTa.Wms.PickLists
{
    public interface IPickListRepository : IRepository<PickList, Guid>
    {
        Task<PickList> FindByPickListIdAsync(
            Guid pickListId,
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default);

        Task<PickList> FindByPickListCodeAsync(
            string pickListCode,
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default);

        Task<List<PickList>> GetPickListsByPickTypeAsync(
            int type,
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default);

        Task<List<PickList>> GetPickListsByDepartmentCodeAsync(
            string departmentCode,
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default);

        Task<List<PickList>> GetPickListsByPickTypeAndPickBatchAsync(
            int type,
            string pickBatchTip,
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default);

        Task<List<PickList>> GetAllPickListsAsync(
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default);

        Task<List<PickList>> GetAllUnFinishedPickListsAsync(
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default);


    }
}
 