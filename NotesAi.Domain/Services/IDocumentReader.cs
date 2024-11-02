using System.Threading;
using System.Threading.Tasks;
using NotesAi.Domain.Aggregates;

namespace NotesAi.Domain.Services;

public interface IDocumentReader<TDocumentReference>
{
    public Task<Document> ReadNewDocument(TDocumentReference documentReference, CancellationToken cancellationToken);

    public Task<Document> ReadUpdateIntoDocument(
        TDocumentReference documentReference,
        Document document,
        CancellationToken cancellationToken
    );
}
