using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NotesAi.Domain.Aggregates;
using NotesAi.Domain.Repositories;

namespace NotesAi.Domain.Services;

public class DocumentService<TDocumentInfo>(
    ILogger<DocumentService<TDocumentInfo>> logger,
    IDocumentRepository documentRepo,
    IDocumentReader<TDocumentInfo> documentReader,
    IEmbeddingService embeddingService
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
                    var embeddings = await GetEmbeddings(
                        updatedDocument.Paragraphs.Select(p => p.Text),
                        cancellationToken
                    );
                    var success = await documentRepo.UpdateDocument(updatedDocument, embeddings, cancellationToken);
                    if (!success)
                    {
                        logger.LogWarning("Tried to update non-existent document {Id}", updatedDocument.Id);
                    }
                }
            }
            else
            {
                var document = await documentReader.ReadNewDocument(documentInfo, cancellationToken);
                var embeddings = await GetEmbeddings(document.Paragraphs.Select(p => p.Text), cancellationToken);
                await documentRepo.CreateDocument(document, embeddings, cancellationToken);
            }
        }
    }

    private async Task<IReadOnlyList<ReadOnlyMemory<float>>> GetEmbeddings(
        IEnumerable<string> texts,
        CancellationToken cancellationToken
    )
    {
        var textsArray = texts.ToArray();
        var overlappingTexts = textsArray
            .Prepend("")
            .Zip(textsArray, (pre, curr) => string.IsNullOrWhiteSpace(pre) ? curr : $"{pre}\n{curr}")
            .Zip(
                textsArray.Skip(1).Append(""),
                (curr, post) => string.IsNullOrWhiteSpace(post) ? curr : $"{curr}\n{post}"
            );
        var embeddings = await embeddingService.GetEmbeddings(overlappingTexts, cancellationToken);
        return embeddings;
    }

    public async IAsyncEnumerable<(Document, int)> SearchDocuments(
        string query,
        int count,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        if (await embeddingService.GetEmbeddings([query], cancellationToken) is not [var queryEmbedding])
        {
            logger.LogError("Could not produce an embedding for {}", query);
            yield break;
        }

        await foreach (
            var (document, matchIndex) in documentRepo.ReadDocumentsForEmbedding(
                queryEmbedding,
                count,
                cancellationToken
            )
        )
        {
            yield return (document, matchIndex);
        }
    }
}

public interface IDocumentInfo
{
    public string Name { get; }
    public DateTimeOffset LatestUpdate { get; }
}
