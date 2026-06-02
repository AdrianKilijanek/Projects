using Microsoft.EntityFrameworkCore;
using WorkerService.Models;

namespace WorkerService.Data;

/// <summary>
/// Kontekst EF Core dla WorkerService. Tylko odczyt i aktualizacja tabeli Prompts.
/// Migracje są zarządzane przez PromptApi.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Prompt> Prompts => Set<Prompt>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Prompt>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasMaxLength(20);
        });
    }
}
