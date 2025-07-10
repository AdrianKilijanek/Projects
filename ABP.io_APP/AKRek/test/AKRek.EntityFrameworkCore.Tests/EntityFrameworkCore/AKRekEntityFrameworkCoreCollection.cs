using Xunit;

namespace AKRek.EntityFrameworkCore;

[CollectionDefinition(AKRekTestConsts.CollectionDefinitionName)]
public class AKRekEntityFrameworkCoreCollection : ICollectionFixture<AKRekEntityFrameworkCoreFixture>
{

}
