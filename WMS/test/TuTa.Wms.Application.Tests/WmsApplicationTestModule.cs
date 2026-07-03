using Volo.Abp.Modularity;

namespace TuTa.Wms;

[DependsOn(
    typeof(WmsApplicationModule),
    typeof(WmsDomainTestModule)
)]
public class WmsApplicationTestModule : AbpModule
{

}
