using Volo.Abp.Ui.Branding;
using Volo.Abp.DependencyInjection;

namespace TuTa.Wms.Web;

[Dependency(ReplaceServices = true)]
public class WmsBrandingProvider : DefaultBrandingProvider
{
    public override string AppName => "Wms";
}
