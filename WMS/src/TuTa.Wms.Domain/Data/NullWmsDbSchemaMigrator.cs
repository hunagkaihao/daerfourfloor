using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace TuTa.Wms.Data;

/* This is used if database provider does't define
 * IWmsDbSchemaMigrator implementation.
 */
public class NullWmsDbSchemaMigrator : IWmsDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
