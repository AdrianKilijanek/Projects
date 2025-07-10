using AKRek.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace AKRek.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AKRekEntityFrameworkCoreModule),
    typeof(AKRekApplicationContractsModule)
)]
public class AKRekDbMigratorModule : AbpModule
{
}
