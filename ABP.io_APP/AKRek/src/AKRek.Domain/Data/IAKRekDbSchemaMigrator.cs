using System.Threading.Tasks;

namespace AKRek.Data;

public interface IAKRekDbSchemaMigrator
{
    Task MigrateAsync();
}
