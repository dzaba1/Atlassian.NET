using System;
using Atlassian.Jira.Remote;

namespace Atlassian.Jira;

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
    public DateTime? StartDate { get; set; }

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
    public DateTime? CreateDate { get; private set; }

    /// <summary>
    /// Gets the date and time the worklog entry was last updated.
    /// </summary>
    public DateTime? UpdateDate { get; private set; }

    /// <summary>
    /// Creates a new worklog instance
    /// </summary>
    /// <param name="timeSpent">Specifies a time duration in JIRA duration format, representing the time spent working</param>
    /// <param name="startDate">When the work was started</param>
    /// <param name="comment">An optional comment to describe the work</param>
    public Worklog(string timeSpent, DateTime startDate, string comment = null)
    {
        TimeSpent = timeSpent;
        StartDate = startDate;
        Comment = comment;
    }

    internal Worklog(RemoteWorklog remoteWorklog)
    {
        if (remoteWorklog != null)
        {
            Author = remoteWorklog.authorUser?.InternalIdentifier;
            AuthorUser = remoteWorklog.authorUser;
            Comment = remoteWorklog.comment;
            StartDate = remoteWorklog.startDate;
            TimeSpent = remoteWorklog.timeSpent;
            Id = remoteWorklog.id;
            CreateDate = remoteWorklog.created;
            TimeSpentInSeconds = remoteWorklog.timeSpentInSeconds;
            UpdateDate = remoteWorklog.updated;
        }
    }

    internal RemoteWorklog ToRemote()
    {
        return new RemoteWorklog()
        {
            authorUser = Author == null ? null : new JiraUser() { InternalIdentifier = Author },
            comment = Comment,
            startDate = StartDate,
            timeSpent = TimeSpent
        };
    }
}
