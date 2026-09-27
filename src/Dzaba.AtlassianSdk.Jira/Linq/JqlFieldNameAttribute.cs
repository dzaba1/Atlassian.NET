using System;

namespace Dzaba.AtlassianSdk.Jira.Linq;

/// <summary>
/// Attribute that can be applied to properties that map to different JQL field names
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
internal class JqlFieldNameAttribute: Attribute
{
    public string Name { get; private set; }

    public JqlFieldNameAttribute(string name)
    {
        Name = name;
    }
}
