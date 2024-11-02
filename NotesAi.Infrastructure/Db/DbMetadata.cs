using System.Collections.Generic;

namespace NotesAi.Infrastructure.Db;

public record DbMetadata
{
    public required ICollection<DbMetadataProperty>? Properties { get; init; }
}
