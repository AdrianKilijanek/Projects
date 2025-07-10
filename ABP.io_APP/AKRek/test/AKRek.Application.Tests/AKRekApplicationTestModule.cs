using Volo.Abp.Modularity;

namespace AKRek;

[DependsOn(
    typeof(AKRekApplicationModule),
    typeof(AKRekDomainTestModule)
)]
public class AKRekApplicationTestModule : AbpModule
{

}
