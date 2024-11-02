using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace NotesAi.Infrastructure.Db;

public class DocumentDbContext(IConfiguration configuration) : DbContext
{
    private readonly string connectionString =
        configuration.GetConnectionString("DocumentDatabase") ?? throw new NoConnectionStringException();

    public required DbSet<DbDocument> Documents { get; init; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var documentEntity = modelBuilder.Entity<DbDocument>();
        documentEntity.HasKey(d => d.Id);
        documentEntity.HasIndex(d => d.Name).IsUnique();
        documentEntity.OwnsMany(
            d => d.Paragraphs,
            p =>
            {
                p.Ignore(p => p.DocumentId);
                p.WithOwner().HasForeignKey(p => p.DocumentId);
                p.HasKey(p => new { p.DocumentId, p.Index });
            }
        );
        documentEntity.OwnsOne(
            d => d.Metadata,
            m =>
                m.OwnsMany(
                    m => m.Properties,
                    p =>
                    {
                        p.Ignore(p => p.DocumentId);
                        p.WithOwner().HasForeignKey(p => p.DocumentId);
                        p.HasKey(p => new { p.DocumentId, p.Key });
                    }
                )
        );
    }

    private class NoConnectionStringException : Exception;
}

public class DocumentDbContextFactory : IDesignTimeDbContextFactory<DocumentDbContext>
{
    public DocumentDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection([new("ConnectionStrings:DocumentDatabase", "Data Source=documents.db")])
            .Build();

        return new DocumentDbContext(configuration) { Documents = null! };
    }
}
