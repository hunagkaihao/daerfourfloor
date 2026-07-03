using Volo.Abp.Modularity;

namespace TuTa.Wms;

[DependsOn(
    typeof(WmsDomainModule),
    typeof(WmsTestBaseModule)
)]
public class WmsDomainTestModule : AbpModule
{

}
