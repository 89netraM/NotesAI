using System;

namespace NotesAi.Infrastructure.Db;

public record DbMetadataProperty
{
    public Guid DocumentId { get; init; }
    public required string Key { get; init; }
    public required string Value { get; init; }
}
