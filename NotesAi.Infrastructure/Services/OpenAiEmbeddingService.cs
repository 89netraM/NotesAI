using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NotesAi.Domain.Services;
using OpenAI.Embeddings;

namespace NotesAi.Infrastructure.Services;

public class OpenAiEmbeddingService(IOptions<OpenAiConfig> config) : IEmbeddingService
{
    private readonly EmbeddingClient embeddingClient =
        new(config.Value.EmbeddingModel, new(config.Value.ApiKey), new() { Endpoint = config.Value.Endpoint });

    public async Task<IReadOnlyList<ReadOnlyMemory<float>>> GetEmbeddings(
        IEnumerable<string> texts,
        CancellationToken cancellationToken
    )
    {
        var embeddingResponse = await embeddingClient.GenerateEmbeddingsAsync(
            texts,
            new() { Dimensions = 256 },
            cancellationToken
        );
        return embeddingResponse.Value.Select(embedding => embedding.ToFloats()).ToList();
    }
}
