using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NotesAi.Domain.Aggregates;
using NotesAi.Domain.Aggregates.Entities;
using NotesAi.Domain.Services;

namespace NotesAi.Infrastructure.Services;

public class FileDocumentReader(
    MarkdownContentReader markdownDocumentReader,
    PlaintextContentReader plaintextContentReader
) : IDocumentReader<FileDocumentInfo>
{
    public async Task<Document> ReadNewDocument(FileDocumentInfo documentInfo, CancellationToken cancellationToken)
    {
        var (paragraphs, metadata) = await ReadDocumentFile(documentInfo.FileInfo, cancellationToken);
        return new Document
        {
            Name = documentInfo.Name,
            Paragraphs = paragraphs.ToArray(),
            Metadata = metadata,
            LatestUpdate = documentInfo.LatestUpdate,
        };
    }

    public async Task<Document> ReadUpdateIntoDocument(
        FileDocumentInfo documentInfo,
        Document document,
        CancellationToken cancellationToken
    )
    {
        var (paragraphs, metadata) = await ReadDocumentFile(documentInfo.FileInfo, cancellationToken);
        return document.Update(paragraphs, metadata, documentInfo.LatestUpdate);
    }

    private async Task<(IEnumerable<Paragraph>, Metadata)> ReadDocumentFile(
        FileInfo fileInfo,
        CancellationToken cancellationToken
    ) =>
        markdownDocumentReader.IsMarkdownFile(fileInfo)
            ? await markdownDocumentReader.ReadContent(fileInfo, cancellationToken)
            : await plaintextContentReader.ReadContent(fileInfo, cancellationToken);
}

public record FileDocumentInfo(FileInfo FileInfo) : IDocumentInfo
{
    public string Name => FileInfo.Name;

    public DateTimeOffset LatestUpdate => new(FileInfo.LastWriteTimeUtc);
}
