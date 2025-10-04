namespace Atlassian.Jira;

/// <summary>
/// Represents a link between an issue and a remote link.
/// </summary>
public class IssueRemoteLink
{
    private readonly string _remoteUrl;
    private readonly string _title;
    private readonly string _summary;

    /// <summary>
    /// Creates a new IssueRemoteLink instance.
    /// </summary>
    public IssueRemoteLink(string remoteUrl, string title, string summary)
    {
        _remoteUrl = remoteUrl;
        _title = title;
        _summary = summary;
    }

    /// <summary>
    /// The remote url of the link relationship.
    /// </summary>
    public string RemoteUrl
    {
        get { return _remoteUrl; }
    }

    /// <summary>
    /// The title / link text.
    /// </summary>
    public string Title
    {
        get { return _title; }
    }

    /// <summary>
    /// The summary / comment.
    /// </summary>
    public string Summary
    {
        get { return _summary; }
    }

}
