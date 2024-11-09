using Microsoft.Extensions.DependencyInjection;
using NotesAi.Domain.Repositories;
using NotesAi.Domain.Services;
using NotesAi.Infrastructure.Db;
using NotesAi.Infrastructure.Repositories;
using NotesAi.Infrastructure.Services;

namespace NotesAi.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSqliteDocumentRepository(this IServiceCollection services) =>
        services.AddDbContext<DocumentDbContext>().AddScoped<IDocumentRepository, DocumentRepository>();

    public static IServiceCollection AddDocumentService(this IServiceCollection services) =>
        services
            .AddSingleton<PlaintextContentReader>()
            .AddSingleton<MarkdownContentReader>()
            .AddSingleton<IDocumentReader<FileDocumentInfo>, FileDocumentReader>()
            .AddSingleton<DocumentService<FileDocumentInfo>>();

    public static IServiceCollection AddOpenAiServices(this IServiceCollection services)
    {
        services.AddOptions<OpenAiConfig>().BindConfiguration("OpenAi");
        services.AddSingleton<IEmbeddingService, OpenAiEmbeddingService>();
        return services;
    }
}
