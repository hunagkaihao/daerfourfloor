using System.Threading.Tasks;

namespace TuTa.Wms.Data;

public interface IWmsDbSchemaMigrator
{
    Task MigrateAsync();
}
