using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using TuTa.Wms.EntityFrameworkCore;
using TuTa.Wms.Erp;
using TuTa.Wms.Erp.Entities;
using Wms.EntityFrameworkCore;
using System.Collections.Generic;

namespace TuTa.Wms.Repositories.Erp
{
    public class EfCoreErpWarehouseAreaPrdtRepository : EfCoreRepository<ErpDbContext, ErpWarehouseAreaPrdt>, IErpWarehouseAreaPrdtRepository
    {
        public EfCoreErpWarehouseAreaPrdtRepository(IDbContextProvider<ErpDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        public async Task<string> GetAreaByPrdtName(
            string name,string dept,
            bool isTrack = true,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .Where(o => o.PRDT_NAME == name && o.DEPT_ID == dept)
                .Select(t => t.CLCHKLB_ID)
                .FirstOrDefaultAsync();
        }
    }
}
