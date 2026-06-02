using Microsoft.EntityFrameworkCore;
using PromptApi.Models;

namespace PromptApi.Data;

/// <summary>
/// Kontekst EF Core dla PostgreSQL. Właściciel schematu bazy i migracji.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Prompt> Prompts => Set<Prompt>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Prompt>(entity =>
        {
            entity.HasKey(e => e.Id);
            // UUID generowany po stronie bazy, nie aplikacji.
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.Text).IsRequired();

            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("pending");

            // Nazwa kolumny musi być w cudzysłowie — Npgsql tworzy ją jako "Status" (PascalCase, case-sensitive).
            entity.ToTable(t => t.HasCheckConstraint(
                "CK_Prompts_Status",
                "\"Status\" IN ('pending','processing','completed','failed')"));

            entity.HasIndex(e => e.Status).HasDatabaseName("idx_prompts_status");
            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("idx_prompts_created_at")
                .IsDescending();
        });
    }
}
