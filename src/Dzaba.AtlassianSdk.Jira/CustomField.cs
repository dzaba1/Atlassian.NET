using Dzaba.AtlassianSdk.Jira.Model.V3;

namespace Dzaba.AtlassianSdk.Jira;

/// <summary>
/// Represents a custom field defined on a JIRA instance.
/// </summary>
public class CustomField : JiraNamedEntity
{
    private readonly Field _remoteField;

    /// <summary>
    /// Creates an instance of a CustomField from a remote field definition.
    /// </summary>
    public CustomField(Field remoteField)
        : base(remoteField.Id, remoteField.Name)
    {
        _remoteField = remoteField;

        if (string.IsNullOrEmpty(Id) && CustomIdentifier != null)
        {
            Id = $"customfield_{CustomIdentifier}";
        }
    }

    internal Field RemoteField
    {
        get
        {
            return _remoteField;
        }
    }

    /// <summary>
    /// Gets the custom field type identifier (e.g. "com.atlassian.jira.plugin.system.customfieldtypes:textfield").
    /// </summary>
    public string CustomType => _remoteField.Schema?.Custom;

    /// <summary>
    /// Gets the numeric identifier of the custom field.
    /// </summary>
    public long? CustomIdentifier => _remoteField.Schema?.CustomId;
}
