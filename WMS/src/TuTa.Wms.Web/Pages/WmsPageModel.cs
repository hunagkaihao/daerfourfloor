using TuTa.Wms.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace TuTa.Wms.Web.Pages;

public abstract class WmsPageModel : AbpPageModel
{
    protected WmsPageModel()
    {
        LocalizationResourceType = typeof(WmsResource);
    }
}
