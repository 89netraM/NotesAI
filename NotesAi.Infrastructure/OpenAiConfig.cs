using System;

namespace NotesAi.Infrastructure;

public class OpenAiConfig
{
    public required Uri Endpoint { get; init; }
    public string ApiKey { get; init; } = string.Empty;
    public required string EmbeddingModel { get; init; }
}
