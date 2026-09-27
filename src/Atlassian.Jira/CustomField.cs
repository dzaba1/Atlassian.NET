using System;
using Atlassian.Jira.Remote;

namespace Atlassian.Jira;

/// <summary>
/// Represents a custom field defined on a JIRA instance.
/// </summary>
public class CustomField : JiraNamedEntity
{
    private readonly RemoteField _remoteField;

    /// <summary>
    /// Creates an instance of a CustomField from a remote field definition.
    /// </summary>
    public CustomField(RemoteField remoteField)
        : base(remoteField)
    {
        _remoteField = remoteField;

        if (string.IsNullOrEmpty(Id) && !string.IsNullOrEmpty(CustomIdentifier))
        {
            Id = $"customfield_{CustomIdentifier}";
        }
    }

    internal RemoteField RemoteField
    {
        get
        {
            return _remoteField;
        }
    }

    /// <summary>
    /// Gets the custom field type identifier (e.g. "com.atlassian.jira.plugin.system.customfieldtypes:textfield").
    /// </summary>
    public string CustomType
    {
        get
        {
            return _remoteField.Schema?.Custom;
        }
    }

    /// <summary>
    /// Gets the numeric identifier of the custom field.
    /// </summary>
    public string CustomIdentifier
    {
        get
        {
            return _remoteField.Schema?.CustomId;
        }
    }
}
