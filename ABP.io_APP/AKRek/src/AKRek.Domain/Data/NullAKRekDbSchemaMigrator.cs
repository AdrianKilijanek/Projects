using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace AKRek.Data;

/* This is used if database provider does't define
 * IAKRekDbSchemaMigrator implementation.
 */
public class NullAKRekDbSchemaMigrator : IAKRekDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
