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
    public class EfCoreErpDeptTypeRepository : EfCoreRepository<ErpDbContext, ErpDeptType>, IErpDeptTypeRepository
    {
        public EfCoreErpDeptTypeRepository(IDbContextProvider<ErpDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        public async Task<string> GetIdByName(string name, CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet.AsNoTracking()
                .Where(o => o.CLCHKLB_NAME==name)
                .Select(t=>t.CLCHKLB_ID)
                .FirstOrDefaultAsync();
        }
    }
}
