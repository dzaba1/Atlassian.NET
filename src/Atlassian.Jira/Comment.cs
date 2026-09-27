using System;
using Atlassian.Jira.Remote;
using System.Collections.Generic;
using System.Linq;

namespace Atlassian.Jira;

/// <summary>
/// A comment associated with an issue
/// </summary>
public class Comment
{
    private readonly IEnumerable<RemoteCommentProperty> _properties;
    private Dictionary<string, object> _propertiesMap;

    /// <summary>
    /// Create a new Comment.
    /// </summary>
    public Comment() :
        this(new RemoteComment())
    {
    }

    /// <summary>
    /// Create a new Comment from a remote instance object.
    /// </summary>
    /// <param name="remoteComment">The remote comment.</param>
    public Comment(RemoteComment remoteComment)
    {
        Id = remoteComment.id;
        Author = remoteComment.authorUser?.InternalIdentifier;
        AuthorUser = remoteComment.authorUser;
        Body = remoteComment.body;
        CreatedDate = remoteComment.created;
        GroupLevel = remoteComment.groupLevel;
        RoleLevel = remoteComment.roleLevel;
        UpdateAuthor = remoteComment.updateAuthorUser?.InternalIdentifier;
        UpdateAuthorUser = remoteComment.updateAuthorUser;
        UpdatedDate = remoteComment.updated;
        Visibility = remoteComment.visibility;
        _properties = remoteComment.properties;
        RenderedBody = remoteComment.renderedBody;
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
    /// Gets or sets the group visibility level of the comment.
    /// </summary>
    public string GroupLevel { get; set; }

    /// <summary>
    /// Gets or sets the role visibility level of the comment.
    /// </summary>
    public string RoleLevel { get; set; }

    /// <summary>
    /// Gets the date and time the comment was created.
    /// </summary>
    public DateTime? CreatedDate { get; private set; }

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
    public DateTime? UpdatedDate { get; private set; }

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
    public IReadOnlyDictionary<string, object> Properties
    {
        get
        {
            if (_propertiesMap == null)
            {
                if (_properties == null)
                {
                    _propertiesMap = new Dictionary<string, object>();
                }
                else
                {
                    _propertiesMap = _properties.ToDictionary(prop => prop.key, prop => prop.value);
                }
            }

            return _propertiesMap;
        }

    }

    internal RemoteComment ToRemote()
    {
        return new RemoteComment
        {
            authorUser = Author == null ? null : new JiraUser() { InternalIdentifier = Author },
            body = Body,
            groupLevel = GroupLevel,
            roleLevel = RoleLevel,
            visibility = Visibility
        };
    }
}
