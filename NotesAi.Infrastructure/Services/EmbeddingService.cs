using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using NotesAi.Domain.Services;

namespace NotesAi.Infrastructure.Services;

public class EmbeddingService(IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator) : IEmbeddingService
{
    public async Task<IReadOnlyList<ReadOnlyMemory<float>>> GetEmbeddings(
        IEnumerable<string> texts,
        CancellationToken cancellationToken
    )
    {
        var embeddingResponse = await embeddingGenerator.GenerateAsync(
            texts,
            new() { Dimensions = 768 },
            cancellationToken
        );
        return [.. embeddingResponse.Select(e => e.Vector)];
    }
}
