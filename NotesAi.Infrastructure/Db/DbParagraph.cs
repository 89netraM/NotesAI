using System;

namespace NotesAi.Infrastructure.Db;

public record DbParagraph
{
    public Guid DocumentId { get; init; }
    public required int Index { get; init; }
    public required string Text { get; init; }
}
