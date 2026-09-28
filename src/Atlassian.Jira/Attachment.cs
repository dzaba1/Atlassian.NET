using System;
using System.IO;
using Atlassian.Jira.Remote;

namespace Atlassian.Jira;

/// <summary>
/// An attachment associated with an issue
/// </summary>
public class Attachment
{
    private readonly IJira _jira;

    /// <summary>
    /// Creates a new instance of an Attachment from a remote entity.
    /// </summary>
    /// <param name="jira">Object used to interact with JIRA.</param>
    /// <param name="remoteAttachment">Remote attachment entity.</param>
    public Attachment(IJira jira, RemoteAttachment remoteAttachment)
    {
        _jira = jira;

        AuthorUser = remoteAttachment.authorUser;
        CreatedDate = remoteAttachment.created;
        FileName = remoteAttachment.filename;
        MimeType = remoteAttachment.mimetype;
        FileSize = remoteAttachment.filesize;
        Id = remoteAttachment.id;
    }

    /// <summary>
    /// Id of attachment
    /// </summary>
    public string Id { get; private set; }

    /// <summary>
    /// Author of attachment (user that uploaded the file)
    /// </summary>
    public string Author
    {
        get
        {
            return AuthorUser?.InternalIdentifier;
        }
    }

    /// <summary>
    /// User object of the author of attachment.
    /// </summary>
    public JiraUser AuthorUser { get; private set; }

    /// <summary>
    /// Date of creation
    /// </summary>
    public DateTime? CreatedDate { get; private set; }

    /// <summary>
    /// File name of the attachment
    /// </summary>
    public string FileName { get; private set; }

    /// <summary>
    /// Mime type
    /// </summary>
    public string MimeType { get; private set; }

    /// <summary>
    /// File size
    /// </summary>
    public long? FileSize { get; private set; }

    /// <summary>
    /// Downloads attachment as a byte array.
    /// </summary>
    public byte[] DownloadData()
    {
        return _jira.Attachments.DownloadData(this);
    }

    /// <summary>
    /// Downloads attachment to specified file
    /// </summary>
    /// <param name="fullFileName">Full file name where attachment will be downloaded</param>
    public void Download(FileInfo fullFileName)
    {
        _jira.Attachments.Download(this, fullFileName);
    }
}
