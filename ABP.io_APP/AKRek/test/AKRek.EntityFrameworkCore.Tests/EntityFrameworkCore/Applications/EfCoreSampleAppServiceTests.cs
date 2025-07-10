using AKRek.Customers;
using AKRek.Samples;
using Xunit;

namespace AKRek.EntityFrameworkCore.Applications;

[Collection(AKRekTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : CustomerAppService_Tests<AKRekEntityFrameworkCoreTestModule>
{

}
