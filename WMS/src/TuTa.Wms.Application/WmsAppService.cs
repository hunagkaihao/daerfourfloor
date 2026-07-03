using System;
using System.Collections.Generic;
using System.Text;
using TuTa.Wms.Localization;
using Volo.Abp.Application.Services;

namespace TuTa.Wms;

/* Inherit your application services from this class.
 */
public abstract class WmsAppService : ApplicationService
{
    protected WmsAppService()
    {
        LocalizationResource = typeof(WmsResource);
    }
}
