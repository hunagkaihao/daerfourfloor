using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TuTa.Wms.EntityFrameworkCore;
using TuTa.Wms.Erp;
using TuTa.Wms.Erp.Entities;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace TuTa.Wms.Repositories.Erp
{
    public class EfCoreErpDeptTypeDetailRepository : EfCoreRepository<ErpDbContext, ErpDeptTypeDetail>, IErpDeptTypeDetailRepository
    {
        public EfCoreErpDeptTypeDetailRepository(IDbContextProvider<ErpDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        public async Task<ErpDeptTypeDetail> FindByDeptMaterial(string dept, string material, CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet.AsNoTracking()
                .Where(o => o.DEPT_ID == dept && o.PRDT_ID == material)
                .FirstOrDefaultAsync();
        }
    }
}
