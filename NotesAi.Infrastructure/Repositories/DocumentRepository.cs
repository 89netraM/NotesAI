using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NotesAi.Domain.Aggregates;
using NotesAi.Domain.Aggregates.Entities;
using NotesAi.Domain.Repositories;
using NotesAi.Infrastructure.Db;

namespace NotesAi.Infrastructure.Repositories;

public class DocumentRepository(DocumentDbContext dbContext) : IDocumentRepository
{
    public async Task CreateDocument(Document document, CancellationToken cancellationToken)
    {
        dbContext.Documents.Add(MapDocumentToDbModel(document));
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public IAsyncEnumerable<Document> ReadDocumentsWithNames(
        IEnumerable<string> documentNames,
        CancellationToken cancellationToken
    ) =>
        dbContext
            .Documents.AsNoTracking()
            .Include(d => d.Paragraphs!)
            .Include(d => d.Metadata!)
            .ThenInclude(m => m.Properties!)
            .Where(d => documentNames.Contains(d.Name))
            .AsAsyncEnumerable()
            .Select(MapDocumentToDomainModel);

    public async Task<bool> UpdateDocument(Document document, CancellationToken cancellationToken)
    {
        var dbDocument = await dbContext.Documents.FindAsync(document.Id, cancellationToken);
        if (dbDocument is null)
        {
            return false;
        }
        MapDocumentOntoDbModel(document, dbDocument);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task DeleteUnlistedDocumentsWithNames(
        IEnumerable<string> documentNames,
        CancellationToken cancellationToken
    )
    {
        var toBeRemoved = dbContext.Documents.Where(d => !documentNames.Contains(d.Name));
        dbContext.Documents.RemoveRange(toBeRemoved);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static DbDocument MapDocumentToDbModel(Document document) =>
        new()
        {
            Id = document.Id,
            Name = document.Name,
            Paragraphs = document.Paragraphs.Select(MapParagraphToDbModel).ToList(),
            Metadata = MapMetadataToDbModel(document.Metadata),
            LatestUpdate = document.LatestUpdate,
        };

    private static DbParagraph MapParagraphToDbModel(Paragraph paragraph, int i) =>
        new() { Index = i + 1, Text = paragraph.Text };

    private static DbMetadata MapMetadataToDbModel(Metadata metadata) =>
        new()
        {
            Properties = metadata.Select(kvp => new DbMetadataProperty() { Key = kvp.Key, Value = kvp.Value }).ToList(),
        };

    private static void MapDocumentOntoDbModel(Document document, DbDocument dbDocument)
    {
        dbDocument.Paragraphs = document.Paragraphs.Select(MapParagraphToDbModel).ToList();
        dbDocument.Metadata = MapMetadataToDbModel(document.Metadata);
        dbDocument.LatestUpdate = document.LatestUpdate;
    }

    private static Document MapDocumentToDomainModel(DbDocument dbDocument) =>
        new()
        {
            Id = dbDocument.Id ?? throw new MissingDocumentIdException(),
            Name = dbDocument.Name,
            Paragraphs = MapParagraphsToDomainModel(dbDocument.Paragraphs ?? throw new MissingParagraphsException()),
            Metadata = dbDocument.Metadata is { } dbMetadata
                ? MapMetadataToDomainModel(dbMetadata)
                : new(new Dictionary<string, string>()),
            LatestUpdate = dbDocument.LatestUpdate,
        };

    private static IReadOnlyList<Paragraph> MapParagraphsToDomainModel(ICollection<DbParagraph> dbParagraphs) =>
        dbParagraphs.OrderBy(p => p.Index).Select(p => new Paragraph(p.Text)).ToArray();

    private static Metadata MapMetadataToDomainModel(DbMetadata dbMetadata) =>
        new(
            dbMetadata.Properties?.ToDictionary(p => p.Key, p => p.Value)
                ?? throw new MissingMetadataPropertiesException()
        );

    private class MissingDocumentIdException : Exception;

    private class MissingParagraphsException : Exception;

    private class MissingMetadataException : Exception;

    private class MissingMetadataPropertiesException : Exception;
}
