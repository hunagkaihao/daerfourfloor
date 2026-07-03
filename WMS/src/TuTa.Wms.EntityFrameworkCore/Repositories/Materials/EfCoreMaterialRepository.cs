using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using TuTa.Wms.Domain;
using TuTa.Wms.EntityFrameworkCore;
using TuTa.Wms.Materials;
using TuTa.Wms.Materials.Aggregates;
using Wms.EntityFrameworkCore;

namespace Wms.Repositories.Materials
{
    public class EfCoreMaterialRepository : EfCoreRepository<WmsDbContext, Material, Guid>, IMaterialRepository
    {
        public EfCoreMaterialRepository(IDbContextProvider<WmsDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        public async Task<Material> FindByMaterialCodeAsync(string materialCode, CancellationToken cancellationToken = default)
        {
            DbSet<Material> dbSet = await GetDbSetAsync();
            return await dbSet
                .AsNoTracking()
                .Where(o => o.MaterialCode == materialCode)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<Material> FindByMaterialNameAndSpecsAsync(
            string materialName,
            string specs,
            CancellationToken cancellationToken = default)
        {
            DbSet<Material> dbSet = await GetDbSetAsync();
            return await dbSet
                .AsNoTracking()
                .Where(o => o.MaterialName == materialName && o.Specs == specs)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<Material>> GetMaterialsByCodeTipAsync(
            string materialCodeTip,
            bool isTrack = true,
            CancellationToken cancellationToken = default)
        {
            DbSet<Material> dbSet = await GetDbSetAsync();
            return await dbSet
                .TrackIf(isTrack)
                .Where(o => (materialCodeTip == null ? true : o.MaterialCode.Contains(materialCodeTip)))
                .OrderBy(o => o.Id)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<QueryDataInPage<Material>> GetPagedMaterialsAsync(
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
            CancellationToken cancellationToken = default)
        {
            DbSet<Material> dbSet = await GetDbSetAsync();
            IQueryable<Material> querable = dbSet.AsNoTracking()
                .Where(o => (materialCode == null ? true : o.MaterialCode == materialCode) &&
                (materialName == null ? true : o.MaterialName == materialName) &&
                (specs == null ? true : o.Specs == specs) &&
                (unit == null ? true : o.Unit == unit) &&
                (typeCode == null ? true : o.TypeCode == typeCode) &&
                (typeName == null ? true : o.TypeName == typeName) &&
                (isHB == null ? true : o.IsHB == isHB) &&
                (safetyStock == null ? true : o.SafetyStock == safetyStock) &&
                (expiryDate == null ? true : o.ExpiryDays == expiryDate) &&
                (isQCPJ == null ? true : o.IsQCPJ == isQCPJ) &&
                (isPPAP == null ? true : o.IsPPAP == isPPAP));
                
            return new QueryDataInPage<Material>()
            {
                TotalCount = await querable.CountAsync(),
                Items = await querable
                    .OrderByDescending(o => o.CreationTime)
                    .PageBy(skipCount, maxResultCount)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false)
            };
        }
    }
}
