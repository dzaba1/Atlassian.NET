using System;

namespace Dzaba.AtlassianSdk.Jira;

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
    public Attachment(IJira jira, Model.V3.Attachment remoteAttachment)
    {
        _jira = jira;

        AuthorUser = new JiraUser(remoteAttachment.Author);
        CreatedDate = remoteAttachment.Created;
        FileName = remoteAttachment.Filename;
        MimeType = remoteAttachment.MimeType;
        FileSize = remoteAttachment.Size;
        Id = remoteAttachment.Id;
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
    public DateTimeOffset CreatedDate { get; private set; }

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
        var url = GetRequestUrl();

        return _jira.RestClient.DownloadData(url);
    }

    /// <summary>
    /// Downloads attachment to specified file
    /// </summary>
    /// <param name="fullFileName">Full file name where attachment will be downloaded</param>
    public void Download(string fullFileName)
    {
        var url = GetRequestUrl();

        _jira.RestClient.Download(url, fullFileName);
    }

    private string GetRequestUrl()
    {
        if (string.IsNullOrEmpty(_jira.Url))
        {
            throw new InvalidOperationException("Unable to download attachment, JIRA url has not been set.");
        }

        // return string.Format("{0}secure/attachment/{1}/{2}",
        //     _jira.Url.EndsWith("/") ? _jira.Url : _jira.Url + "/",
        //     Id,
        //     FileName);
    }
}
