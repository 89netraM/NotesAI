using System;
using System.Collections.Generic;
using System.Linq;
using NotesAi.Domain.Aggregates.Entities;

namespace NotesAi.Domain.Aggregates;

public record Document
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Name { get; init; }
    public required IReadOnlyList<Paragraph> Paragraphs { get; init; }
    public required Metadata Metadata { get; init; }
    public required DateTimeOffset LatestUpdate { get; init; }

    public Document Update(IEnumerable<Paragraph> paragraphs, Metadata metadata, DateTimeOffset latestUpdate) =>
        this with
        {
            Paragraphs = paragraphs.ToArray(),
            Metadata = metadata,
            LatestUpdate = latestUpdate,
        };
}
