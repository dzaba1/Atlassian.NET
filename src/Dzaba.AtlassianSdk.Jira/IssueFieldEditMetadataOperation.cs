using System.Text.Json.Serialization;

namespace Dzaba.AtlassianSdk.Jira;

/// <summary>
/// Possible values of operations property in IssueFieldEditMetadata.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IssueFieldEditMetadataOperation
{
    /// <summary>
    /// The field's value can be set, replacing any existing value.
    /// </summary>
    SET = 1,

    /// <summary>
    /// A value can be added to the field.
    /// </summary>
    ADD = 2,

    /// <summary>
    /// A value can be removed from the field.
    /// </summary>
    REMOVE = 3,

    /// <summary>
    /// The field's value can be edited.
    /// </summary>
    EDIT = 4
}
