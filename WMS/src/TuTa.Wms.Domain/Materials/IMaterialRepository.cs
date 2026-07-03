using System;
using System.Threading.Tasks;
using System.Threading;
using Volo.Abp.Domain.Repositories;
using TuTa.Wms.Domain;
using TuTa.Wms.Materials.Aggregates;
using System.Collections.Generic;

namespace TuTa.Wms.Materials
{
    public interface IMaterialRepository : IRepository<Material, Guid>
    {
        public Task<Material> FindByMaterialCodeAsync(
            string materialCode,
            CancellationToken cancellationToken = default);

        public Task<Material> FindByMaterialNameAndSpecsAsync(
            string materialName,
            string specs,
            CancellationToken cancellationToken = default);

        public Task<List<Material>> GetMaterialsByCodeTipAsync(
            string materialCodeTip,
            bool isTrack = true,
            CancellationToken cancellationToken = default);

        public Task<QueryDataInPage<Material>> GetPagedMaterialsAsync(
            string materialCode,
            string materialName,
            string specs,
            string unit,
            string typeCode,
            string typeName,
            string isHB,
            int? safetyStock,
            int? expiryDate,
            bool? isQCPJ,
            bool? isPPAP,
            int skipCount = 0,
            int maxResultCount = 10,
            CancellationToken cancellationToken = default);
    }
}
