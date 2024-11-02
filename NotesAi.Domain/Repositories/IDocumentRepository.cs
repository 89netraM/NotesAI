using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NotesAi.Domain.Aggregates;

namespace NotesAi.Domain.Repositories;

public interface IDocumentRepository
{
    public Task CreateDocument(Document document, CancellationToken cancellationToken);

    public IAsyncEnumerable<Document> ReadDocumentsWithNames(
        IEnumerable<string> documentNames,
        CancellationToken cancellationToken
    );

    public Task<bool> UpdateDocument(Document document, CancellationToken cancellationToken);

    public Task DeleteUnlistedDocumentsWithNames(
        IEnumerable<string> documentNames,
        CancellationToken cancellationToken
    );
}
