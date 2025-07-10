using Volo.Abp.Modularity;

namespace AKRek;

[DependsOn(
    typeof(AKRekDomainModule),
    typeof(AKRekTestBaseModule)
)]
public class AKRekDomainTestModule : AbpModule
{

}
