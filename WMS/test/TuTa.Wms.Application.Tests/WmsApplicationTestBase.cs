using Volo.Abp.Modularity;

namespace TuTa.Wms;

public abstract class WmsApplicationTestBase<TStartupModule> : WmsTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
