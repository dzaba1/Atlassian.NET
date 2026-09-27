using System;
using Dzaba.AtlassianSdk.Jira.Model.V3;

namespace Dzaba.AtlassianSdk.Jira;

/// <summary>
/// Represents the worklog of an issue
/// </summary>
public class Worklog
{
    /// <summary>
    /// Gets or sets the username of the worklog's author.
    /// </summary>
    public string Author { get; set; }

    /// <summary>
    /// Gets the user who authored the worklog entry.
    /// </summary>
    public JiraUser AuthorUser { get; private set; }

    /// <summary>
    /// Gets or sets an optional comment describing the work.
    /// </summary>
    public string Comment { get; set; }

    /// <summary>
    /// Gets or sets the date and time the work was started.
    /// </summary>
    public DateTimeOffset StartDate { get; set; }

    /// <summary>
    /// Gets or sets the time spent working, in JIRA duration format (e.g. "1d 2h").
    /// </summary>
    public string TimeSpent { get; set; }

    /// <summary>
    /// Gets the identifier of the worklog entry.
    /// </summary>
    public string Id { get; private set; }

    /// <summary>
    /// Gets the time spent working, in seconds.
    /// </summary>
    public long TimeSpentInSeconds { get; private set; }

    /// <summary>
    /// Gets the date and time the worklog entry was created.
    /// </summary>
    public DateTimeOffset CreateDate { get; private set; }

    /// <summary>
    /// Gets the date and time the worklog entry was last updated.
    /// </summary>
    public DateTimeOffset UpdateDate { get; private set; }

    /// <summary>
    /// Creates a new worklog instance
    /// </summary>
    /// <param name="timeSpent">Specifies a time duration in JIRA duration format, representing the time spent working</param>
    /// <param name="startDate">When the work was started</param>
    /// <param name="comment">An optional comment to describe the work</param>
    public Worklog(string timeSpent, DateTimeOffset startDate, string comment = null)
    {
        TimeSpent = timeSpent;
        StartDate = startDate;
        Comment = comment;
    }

    internal Worklog(Model.V3.Worklog remoteWorklog)
    {
        if (remoteWorklog != null)
        {
            AuthorUser = new JiraUser(remoteWorklog.Author);
            Author = AuthorUser?.InternalIdentifier;
            Comment = remoteWorklog.Comment.ToString();
            StartDate = remoteWorklog.Started;
            TimeSpent = remoteWorklog.TimeSpent;
            Id = remoteWorklog.Id;
            CreateDate = remoteWorklog.Created;
            TimeSpentInSeconds = remoteWorklog.TimeSpentSeconds;
            UpdateDate = remoteWorklog.Updated;
        }
    }

    internal Model.V3.Worklog ToRemote()
    {
        return new Model.V3.Worklog()
        {
            Author = Author == null ? null : new UserDetails() { InternalIdentifier = Author },
            Comment = Comment,
            Started = StartDate,
            TimeSpent = TimeSpent
        };
    }
}
