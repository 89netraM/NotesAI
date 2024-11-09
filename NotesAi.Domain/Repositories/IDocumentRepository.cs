using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NotesAi.Domain.Aggregates;

namespace NotesAi.Domain.Repositories;

public interface IDocumentRepository
{
    public Task CreateDocument(
        Document document,
        IEnumerable<ReadOnlyMemory<float>> embeddings,
        CancellationToken cancellationToken
    );

    public IAsyncEnumerable<Document> ReadDocumentsWithNames(
        IEnumerable<string> documentNames,
        CancellationToken cancellationToken
    );

    public IAsyncEnumerable<Document> ReadDocumentsForEmbedding(
        ReadOnlyMemory<float> embedding,
        int count,
        CancellationToken cancellationToken
    );

    public Task<bool> UpdateDocument(
        Document document,
        IEnumerable<ReadOnlyMemory<float>> embeddings,
        CancellationToken cancellationToken
    );

    public Task DeleteUnlistedDocumentsWithNames(
        IEnumerable<string> documentNames,
        CancellationToken cancellationToken
    );
}
