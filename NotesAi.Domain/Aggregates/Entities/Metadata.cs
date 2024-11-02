using System.Collections;
using System.Collections.Generic;

namespace NotesAi.Domain.Aggregates.Entities;

public class Metadata(IReadOnlyDictionary<string, string> properties) : IEnumerable<KeyValuePair<string, string>>
{
    public string? this[string key] => properties.TryGetValue(key, out var metadataValue) ? metadataValue : null;

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => properties.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
