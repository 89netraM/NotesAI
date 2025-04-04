using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace NotesAi.Infrastructure.Db;

public class DocumentDbContext(IConfiguration configuration, ILoggerFactory loggerFactory) : DbContext
{
    private readonly string connectionString =
        configuration.GetConnectionString("DocumentDatabase") ?? throw new NoConnectionStringException();

    public required DbSet<DbDocument> Documents { get; init; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        var connection = new SqliteConnection(connectionString);
        connection.LoadExtension(GetVectorliteExtensionPath());
        options.UseSqlite(connection, options => options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
        options.UseLoggerFactory(loggerFactory);
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
                p.HasKey(p => p.Id);
                p.HasIndex(p => new { p.DocumentId, p.Index }).IsUnique();
                p.OwnsOne(
                    p => p.Vector,
                    v =>
                    {
                        v.WithOwner().HasForeignKey(v => v.RowId);
                        v.HasKey(v => v.RowId);
                        v.ToTable("DbParagraphVector", table => table.ExcludeFromMigrations());
                        v.Property(v => v.RowId).HasColumnName("rowid");
                        v.OwnedEntityType.UseSqlReturningClause(false);
                        var embeddingProperty = v.Property(v => v.Embedding);
                        embeddingProperty.HasColumnName("embedding").HasColumnType("BLOB");
                        embeddingProperty.Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
                    }
                );
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
        documentEntity.Navigation(d => d.Metadata).IsRequired();
    }

    private static string GetVectorliteExtensionPath()
    {
        var rid = RuntimeInformation.RuntimeIdentifier;
        var libFilename = rid switch
        {
            "win-x64" => "vectorlite.dll",
            "osx-arm64" or "osx-x64" => "vectorlite.dylib",
            "linux-x64" => "vectorlite.so",
            _ => throw new UnsupportedVectorlitePlatformException(rid),
        };

        var exePath =
            Assembly.GetExecutingAssembly()?.Location
            ?? throw new NoVectorliteExtensionException("Could not find entry assembly");
        var exeDir =
            Path.GetDirectoryName(exePath)
            ?? throw new NoVectorliteExtensionException($"Could not get parent directory of {exePath}");

        var inExeDirPath = Path.Join(exeDir, libFilename);
        if (File.Exists(inExeDirPath))
        {
            return inExeDirPath;
        }

        var inRuntimePath = Path.Combine(exeDir, "runtimes", rid, "native", libFilename);
        if (File.Exists(inRuntimePath))
        {
            return inRuntimePath;
        }

        throw new NoVectorliteExtensionException(
            $"Could not find {libFilename}, looked in {inExeDirPath} and {inRuntimePath}"
        );
    }

    private class NoConnectionStringException : Exception;

    private class UnsupportedVectorlitePlatformException(string rid)
        : Exception($"Platform \"{rid}\" does not support Vectorlite");

    private class NoVectorliteExtensionException(string message) : Exception(message);
}

public class DocumentDbContextFactory : IDesignTimeDbContextFactory<DocumentDbContext>
{
    public DocumentDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection([new("ConnectionStrings:DocumentDatabase", "Data Source=documents.db")])
            .Build();

        return new DocumentDbContext(configuration, NullLoggerFactory.Instance) { Documents = null! };
    }
}
