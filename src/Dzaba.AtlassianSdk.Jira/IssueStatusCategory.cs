using Dzaba.AtlassianSdk.Jira.Model.V3;

namespace Dzaba.AtlassianSdk.Jira;

/// <summary>
/// The category of an issue status as defined in JIRA.
/// </summary>
public class IssueStatusCategory : JiraNamedEntity
{
    /// <summary>
    /// Creates an instance of the IssueStatusCategory based on a remote entity.
    /// </summary>
    public IssueStatusCategory(StatusCategory remoteStatusCategory)
        : base(remoteStatusCategory.Id.ToString(), remoteStatusCategory.Name)
    {
        ColorName = remoteStatusCategory?.ColorName;
        Key = remoteStatusCategory?.Key;
    }

    /// <summary>
    /// Creates an instance of the IssueStatusCategory based on a remote entity.
    /// </summary>
    public IssueStatusCategory(IssueStatusCategory remoteStatusCategory)
        : base(remoteStatusCategory.Id.ToString(), remoteStatusCategory.Name)
    {
        ColorName = remoteStatusCategory?.ColorName;
        Key = remoteStatusCategory?.Key;
    }

    /// <summary>
    /// The color assigned to this category.
    /// </summary>
    public string ColorName { get; private set; }

    /// <summary>
    /// The key assigned to this category.
    /// </summary>
    public string Key { get; private set; }
}
