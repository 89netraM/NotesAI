namespace NotesAi.Infrastructure;

public class OpenAiConfig
{
    public required string ApiKey { get; init; }
    public required string EmbeddingModel { get; init; }
}
