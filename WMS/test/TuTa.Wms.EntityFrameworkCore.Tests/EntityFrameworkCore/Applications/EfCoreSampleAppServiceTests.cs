using TuTa.Wms.Samples;
using Xunit;

namespace TuTa.Wms.EntityFrameworkCore.Applications;

[Collection(WmsTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<WmsEntityFrameworkCoreTestModule>
{

}
