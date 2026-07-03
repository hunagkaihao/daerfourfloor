using TuTa.Wms.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace TuTa.Wms.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class WmsController : AbpControllerBase
{
    protected WmsController()
    {
        LocalizationResource = typeof(WmsResource);
    }
}
