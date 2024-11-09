using System;

namespace NotesAi.Infrastructure.Db;

public record DbParagraph
{
    public int Id { get; init; }
    public Guid DocumentId { get; init; }
    public required int Index { get; init; }
    public required string Text { get; init; }
    public required DbParagraphVector Vector { get; init; }
}
