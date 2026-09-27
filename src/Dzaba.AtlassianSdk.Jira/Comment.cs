using System;
using System.Collections.Generic;
using System.Linq;
using Dzaba.AtlassianSdk.Jira.Model.V3;

namespace Dzaba.AtlassianSdk.Jira;

/// <summary>
/// A comment associated with an issue
/// </summary>
public class Comment
{
    /// <summary>
    /// Create a new Comment.
    /// </summary>
    public Comment() :
        this(new Model.V3.Comment())
    {
    }

    /// <summary>
    /// Create a new Comment from a remote instance object.
    /// </summary>
    /// <param name="remoteComment">The remote comment.</param>
    public Comment(Model.V3.Comment remoteComment)
    {
        Id = remoteComment.Id;
        AuthorUser = new JiraUser(remoteComment.Author);
        Author = AuthorUser?.InternalIdentifier;
        Body = remoteComment.Body?.ToString();
        CreatedDate = remoteComment.Created;
        UpdateAuthorUser = new JiraUser(remoteComment.UpdateAuthor);
        UpdateAuthor = UpdateAuthorUser?.InternalIdentifier;
        UpdatedDate = remoteComment.Updated;
        Visibility = new CommentVisibility(remoteComment.Visibility);
        Properties = remoteComment.Properties?.ToDictionary(prop => prop.Key, prop => prop.Value);
        RenderedBody = remoteComment.RenderedBody;
    }

    /// <summary>
    /// Gets the identifier of the comment.
    /// </summary>
    public string Id { get; private set; }

    /// <summary>
    /// Gets or sets the username of the comment's author.
    /// </summary>
    public string Author { get; set; }

    /// <summary>
    /// Gets the user who authored the comment.
    /// </summary>
    public JiraUser AuthorUser { get; private set; }

    /// <summary>
    /// Gets or sets the text of the comment.
    /// </summary>
    public string Body { get; set; }

    /// <summary>
    /// Gets the date and time the comment was created.
    /// </summary>
    public DateTimeOffset CreatedDate { get; private set; }

    /// <summary>
    /// Gets the username of the user who last updated the comment.
    /// </summary>
    public string UpdateAuthor { get; private set; }

    /// <summary>
    /// Gets the user who last updated the comment.
    /// </summary>
    public JiraUser UpdateAuthorUser { get; private set; }

    /// <summary>
    /// Gets the date and time the comment was last updated.
    /// </summary>
    public DateTimeOffset UpdatedDate { get; private set; }

    /// <summary>
    /// Gets or sets the visibility restriction applied to the comment.
    /// </summary>
    public CommentVisibility Visibility { get; set; }

    /// <summary>
    /// Gets or sets the rendered (HTML) representation of the comment body.
    /// </summary>
    public string RenderedBody { get; set; }

    /// <summary>
    /// Gets the custom properties attached to the comment.
    /// </summary>
    public IReadOnlyDictionary<string, object> Properties { get; private set;}

    internal Model.V3.Comment ToRemote()
    {
        return new Model.V3.Comment
        {
            Author = Author == null ? null : new UserDetails() { InternalIdentifier = Author },
            Body = Body,
            Visibility = new Visibility
            {
                Type = Visibility.Type,
                Value = Visibility.Value
            }
        };
    }
}
