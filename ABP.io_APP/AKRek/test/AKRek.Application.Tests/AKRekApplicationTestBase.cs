using Volo.Abp.Modularity;

namespace AKRek;

public abstract class AKRekApplicationTestBase<TStartupModule> : AKRekTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
