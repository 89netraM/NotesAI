using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NotesAi.Domain.Repositories;

namespace NotesAi.Domain.Services;

public class DocumentService<TDocumentInfo>(
    ILogger<DocumentService<TDocumentInfo>> logger,
    IDocumentRepository documentRepo,
    IDocumentReader<TDocumentInfo> documentReader
)
    where TDocumentInfo : IDocumentInfo
{
    public async Task UpdateDocumentCollection(
        IEnumerable<TDocumentInfo> documentInfos,
        CancellationToken cancellationToken
    )
    {
        await documentRepo.DeleteUnlistedDocumentsWithNames(documentInfos.Select(d => d.Name), cancellationToken);

        var existingDocuments = await documentRepo
            .ReadDocumentsWithNames(documentInfos.Select(d => d.Name), cancellationToken)
            .ToDictionaryAsync(d => d.Name, cancellationToken);

        foreach (var documentInfo in documentInfos)
        {
            if (existingDocuments.TryGetValue(documentInfo.Name, out var existingDocument))
            {
                if (documentInfo.LatestUpdate > existingDocument.LatestUpdate)
                {
                    var updatedDocument = await documentReader.ReadUpdateIntoDocument(
                        documentInfo,
                        existingDocument,
                        cancellationToken
                    );
                    var success = await documentRepo.UpdateDocument(updatedDocument, cancellationToken);
                    if (!success)
                    {
                        logger.LogWarning("Tried to update non-existent document {Id}", updatedDocument.Id);
                    }
                }
            }
            else
            {
                var document = await documentReader.ReadNewDocument(documentInfo, cancellationToken);
                await documentRepo.CreateDocument(document, cancellationToken);
            }
        }
    }
}

public interface IDocumentInfo
{
    public string Name { get; }
    public DateTimeOffset LatestUpdate { get; }
}
