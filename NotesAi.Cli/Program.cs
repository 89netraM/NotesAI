using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NotesAi.Domain.Services;
using NotesAi.Infrastructure;
using NotesAi.Infrastructure.Db;
using NotesAi.Infrastructure.Services;

var builder = Host.CreateApplicationBuilder(args);

builder
    .Configuration.AddInMemoryCollection(
        [new("ConnectionStrings:DocumentDatabase", @"Data Source=.\.notesai\documents.db")]
    )
    .AddJsonFile(@".\.notesai\appsettings.json", optional: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables()
    .AddCommandLine(args);

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
