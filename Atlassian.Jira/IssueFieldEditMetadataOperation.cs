using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Atlassian.Jira;

/// <summary>
/// Possible values of operations property in IssueFieldEditMetadata.
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum IssueFieldEditMetadataOperation
{
    /// <summary>
    /// The field's value can be set, replacing any existing value.
    /// </summary>
    [EnumMember(Value = "SET")]
    SET = 1,

    /// <summary>
    /// A value can be added to the field.
    /// </summary>
    [EnumMember(Value = "ADD")]
    ADD = 2,

    /// <summary>
    /// A value can be removed from the field.
    /// </summary>
    [EnumMember(Value = "REMOVE")]
    REMOVE = 3,

    /// <summary>
    /// The field's value can be edited.
    /// </summary>
    [EnumMember(Value = "EDIT")]
    EDIT = 4
}
