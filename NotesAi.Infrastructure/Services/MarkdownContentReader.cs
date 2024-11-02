using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Renderers.Roundtrip;
using Markdig.Syntax;
using NotesAi.Domain.Aggregates.Entities;
using YamlDotNet.Serialization;

namespace NotesAi.Infrastructure.Services;

public class MarkdownContentReader
{
    private static readonly MarkdownPipeline markdownPipeline = new MarkdownPipelineBuilder()
        .EnableTrackTrivia()
        .UseYamlFrontMatter()
        .Build();

    private static readonly IDeserializer yamlDeserializer = new DeserializerBuilder().Build();
    private static readonly ISerializer yamlSerializer = new SerializerBuilder().Build();

    public bool IsMarkdownFile(FileInfo fileInfo) =>
        Path.GetExtension(fileInfo.FullName).Equals(".md", StringComparison.InvariantCultureIgnoreCase);

    public async Task<(IEnumerable<Paragraph>, Metadata)> ReadContent(
        FileInfo fileInfo,
        CancellationToken cancellationToken
    )
    {
        var text = await ReadAllText(fileInfo, cancellationToken);
        var markdownDocument = Markdown.Parse(text, markdownPipeline);
        return (ParseParagraphs(text, markdownDocument), ParseMetadata(markdownDocument));
    }

    private static async Task<string> ReadAllText(FileInfo fileInfo, CancellationToken cancellationToken)
    {
        await using var fileStream = fileInfo.OpenRead();
        using var fileReader = new StreamReader(fileStream);
        return await fileReader.ReadToEndAsync(cancellationToken);
    }

    private static IEnumerable<Paragraph> ParseParagraphs(string text, MarkdownDocument markdownDocument)
    {
        foreach (var block in markdownDocument)
        {
            if (block is YamlFrontMatterBlock)
            {
                continue;
            }

            using var stringWriter = new StringWriter();
            var renderer = new RoundtripRenderer(stringWriter);
            renderer.Write(block);
            yield return new(stringWriter.ToString());
        }
    }

    private static Metadata ParseMetadata(MarkdownDocument markdownDocument)
    {
        var frontMatter = markdownDocument.OfType<YamlFrontMatterBlock>().FirstOrDefault();
        if (frontMatter is null)
        {
            return new(new Dictionary<string, string>());
        }
        var metadataProperties = yamlDeserializer.Deserialize<Dictionary<string, object>>(frontMatter.Lines.ToString());
        return new(metadataProperties.ToDictionary(kvp => kvp.Key, kvp => yamlSerializer.Serialize(kvp.Value)));
    }
}
