using AKRek.Samples;
using Xunit;

namespace AKRek.EntityFrameworkCore.Domains;

[Collection(AKRekTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<AKRekEntityFrameworkCoreTestModule>
{

}
