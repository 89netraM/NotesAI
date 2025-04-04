using System;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
        services.AddOptions<OllamaConfig>().BindConfiguration("Ollama");
        services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(sp =>
        {
            var ollamaConfig = sp.GetService<IOptions<OllamaConfig>>()?.Value;
            if (ollamaConfig is not null)
            {
                return new OllamaEmbeddingGenerator(ollamaConfig.Endpoint, ollamaConfig.EmbeddingModel);
            }

            var openAiConfig = sp.GetService<IOptions<OpenAiConfig>>()?.Value;
            if (openAiConfig is not null)
            {
                return new OpenAIEmbeddingGenerator(new(openAiConfig.ApiKey), openAiConfig.EmbeddingModel);
            }

            throw new InvalidOperationException("No valid embedding generator configuration found.");
        });
        services.AddSingleton<IEmbeddingService, EmbeddingService>();
        return services;
    }
}
