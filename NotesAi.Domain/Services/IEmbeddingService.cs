using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NotesAi.Domain.Services;

public interface IEmbeddingService
{
    public Task<IReadOnlyList<ReadOnlyMemory<float>>> GetEmbeddings(
        IEnumerable<string> texts,
        CancellationToken cancellationToken
    );
}
