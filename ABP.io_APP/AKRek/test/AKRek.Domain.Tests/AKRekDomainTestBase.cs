using Volo.Abp.Modularity;

namespace AKRek;

/* Inherit from this class for your domain layer tests. */
public abstract class AKRekDomainTestBase<TStartupModule> : AKRekTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
