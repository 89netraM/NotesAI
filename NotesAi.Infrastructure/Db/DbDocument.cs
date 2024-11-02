using System;
using System.Collections.Generic;

namespace NotesAi.Infrastructure.Db;

public record DbDocument
{
    public required Guid? Id { get; set; }
    public required string Name { get; set; }
    public required ICollection<DbParagraph>? Paragraphs { get; set; }
    public required DbMetadata? Metadata { get; set; }
    public required DateTimeOffset LatestUpdate { get; set; }
}
