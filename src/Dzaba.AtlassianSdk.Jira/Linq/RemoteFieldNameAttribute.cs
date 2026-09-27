using System;

namespace Dzaba.AtlassianSdk.Jira.Linq;

/// <summary>
/// Attribute that can be applied to properties to modify the name of the remotefield used when updating an issue
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
internal class RemoteFieldNameAttribute: Attribute
{
    public string Name { get; private set; }

    public RemoteFieldNameAttribute(string remoteFieldName)
    {
        Name = remoteFieldName;
    }
}
