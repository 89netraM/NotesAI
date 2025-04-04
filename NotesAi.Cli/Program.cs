using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotesAi.Domain.Services;
using NotesAi.Infrastructure;
using NotesAi.Infrastructure.Db;
using NotesAi.Infrastructure.Services;

namespace NotesAi.Cli;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder
            .Configuration.AddInMemoryCollection(
                [new("ConnectionStrings:DocumentDatabase", @"Data Source=.\.notesai\documents.db")]
            )
            .AddJsonFile(@".\.notesai\appsettings.json", optional: true)
            .AddUserSecrets<Program>()
            .AddEnvironmentVariables()
            .AddCommandLine(args);

        builder.Services.AddOptions<CliArguments>().Bind(builder.Configuration);

        builder.Services.AddSqliteDocumentRepository();
        builder.Services.AddDocumentService();
        builder.Services.AddOpenAiServices();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        using (var documentDbContext = scope.ServiceProvider.GetRequiredService<DocumentDbContext>())
        {
            documentDbContext.Database.Migrate();
        }

        var documentService = app.Services.GetRequiredService<DocumentService<FileDocumentInfo>>();

        var documentFiles = Directory
            .EnumerateFiles(".", "*.md", SearchOption.AllDirectories)
            .Select(p => new FileDocumentInfo(new(p)));

        await documentService.UpdateDocumentCollection(documentFiles, CancellationToken.None);

        var logger = app.Services.GetRequiredService<ILogger<Program>>();

        var arguments = app.Services.GetRequiredService<IOptions<CliArguments>>().Value;

        if (arguments.Query is string query)
        {
            await foreach (
                var (document, matchIndex) in documentService.SearchDocuments(query, count: 3, CancellationToken.None)
            )
            {
                logger.LogInformation(
                    "Found matching paragraph in .\\{FileName} paragraph {MatchIndex}",
                    document.Name,
                    matchIndex
                );
                switch (matchIndex - 1 >= 0, matchIndex + 1 < document.Paragraphs.Count)
                {
                    case (true, true):
                        logger.LogInformation(
                            "Surrounding text:\n{Before}{Match}{After}",
                            document.Paragraphs[matchIndex - 1].Text,
                            document.Paragraphs[matchIndex].Text,
                            document.Paragraphs[matchIndex + 1].Text
                        );
                        break;
                    case (true, false):
                        logger.LogInformation(
                            "Surrounding text:\n{Before}{Match}",
                            document.Paragraphs[matchIndex - 1].Text,
                            document.Paragraphs[matchIndex].Text
                        );
                        break;
                    case (false, true):
                        logger.LogInformation(
                            "Surrounding text:\n{Match}{After}",
                            document.Paragraphs[matchIndex].Text,
                            document.Paragraphs[matchIndex + 1].Text
                        );
                        break;
                    case (false, false):
                        logger.LogInformation("Surrounding text:\n{Match}", document.Paragraphs[matchIndex].Text);
                        break;
                }
            }
        }
    }
}
