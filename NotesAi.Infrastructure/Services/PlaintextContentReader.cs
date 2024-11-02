using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NotesAi.Domain.Aggregates.Entities;

namespace NotesAi.Infrastructure.Services;

public class PlaintextContentReader
{
    public async Task<(IEnumerable<Paragraph>, Metadata)> ReadContent(
        FileInfo fileInfo,
        CancellationToken cancellationToken
    )
    {
        var text = await ReadAllText(fileInfo, cancellationToken);
        var paragraphs = text.Split(
                ["\n", "\r\n"],
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
            )
            .Select(p => new Paragraph(p));
        return (paragraphs, new(new Dictionary<string, string>()));
    }

    private static async Task<string> ReadAllText(FileInfo fileInfo, CancellationToken cancellationToken)
    {
        await using var fileStream = fileInfo.OpenRead();
        using var fileReader = new StreamReader(fileStream);
        return await fileReader.ReadToEndAsync(cancellationToken);
    }
}
