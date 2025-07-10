using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AKRek.Data;
using Volo.Abp.DependencyInjection;

namespace AKRek.EntityFrameworkCore;

public class EntityFrameworkCoreAKRekDbSchemaMigrator
    : IAKRekDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreAKRekDbSchemaMigrator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the AKRekDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<AKRekDbContext>()
            .Database
            .MigrateAsync();
    }
}
