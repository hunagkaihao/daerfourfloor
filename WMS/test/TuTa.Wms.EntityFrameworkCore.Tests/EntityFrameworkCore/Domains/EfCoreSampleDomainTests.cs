using TuTa.Wms.Samples;
using Xunit;

namespace TuTa.Wms.EntityFrameworkCore.Domains;

[Collection(WmsTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<WmsEntityFrameworkCoreTestModule>
{

}
