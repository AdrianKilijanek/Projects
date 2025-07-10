using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AKRek.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class AKRekDbContextFactory : IDesignTimeDbContextFactory<AKRekDbContext>
{
    public AKRekDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        
        AKRekEfCoreEntityExtensionMappings.Configure();

        var builder = new DbContextOptionsBuilder<AKRekDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"));
        
        return new AKRekDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../AKRek.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}
