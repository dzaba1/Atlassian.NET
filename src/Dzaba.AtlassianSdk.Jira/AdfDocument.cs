using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dzaba.AtlassianSdk.Jira;

/// <summary>
/// Minimal Atlassian Document Format document, the only body format accepted by the V3 API for comments and worklog comments.
/// </summary>
internal sealed class AdfDocument
{
    [JsonPropertyName("type")]
    public string Type { get; } = "doc";

    [JsonPropertyName("version")]
    public int Version { get; } = 1;

    [JsonPropertyName("content")]
    public IReadOnlyList<AdfNode> Content { get; }

    private AdfDocument(IReadOnlyList<AdfNode> content)
    {
        Content = content;
    }

    /// <summary>
    /// Creates a document consisting of a single paragraph with the given text.
    /// </summary>
    /// <param name="text">The text of the paragraph.</param>
    public static AdfDocument FromText(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        return new AdfDocument([AdfNode.Paragraph(AdfNode.Text(text))]);
    }
}

/// <summary>
/// A node of an <see cref="AdfDocument"/>. Null properties are not serialized.
/// </summary>
internal sealed class AdfNode
{
    [JsonPropertyName("type")]
    public string Type { get; }

    [JsonPropertyName("text")]
    public string TextValue { get; }

    [JsonPropertyName("content")]
    public IReadOnlyList<AdfNode> Content { get; }

    private AdfNode(string type, string text, IReadOnlyList<AdfNode> content)
    {
        Type = type;
        TextValue = text;
        Content = content;
    }

    public static AdfNode Text(string text)
    {
        return new AdfNode("text", text, null);
    }

    public static AdfNode Paragraph(params AdfNode[] content)
    {
        return new AdfNode("paragraph", null, content);
    }
}
