using System;

namespace NotesAi.Infrastructure;

public class OllamaConfig
{
    public required Uri Endpoint { get; init; }
    public required string EmbeddingModel { get; init; }
}
