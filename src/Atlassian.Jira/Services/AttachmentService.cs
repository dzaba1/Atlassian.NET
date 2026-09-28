using System;
using System.IO;

namespace Atlassian.Jira.Services;

internal sealed class AttachmentService : IAttachmentService
{
    private readonly IJira _jira;

    public AttachmentService(IJira jira)
    {
        _jira = jira;
    }

    private string GetRequestUrl(Attachment attachment)
    {
        if (string.IsNullOrEmpty(_jira.Url))
        {
            throw new InvalidOperationException("Unable to download attachment, JIRA url has not been set.");
        }

        return string.Format("{0}secure/attachment/{1}/{2}",
            _jira.Url.EndsWith("/") ? _jira.Url : _jira.Url + "/",
            attachment.Id,
            attachment.FileName);
    }

    public byte[] DownloadData(Attachment attachment)
    {
        var url = GetRequestUrl(attachment);

        return _jira.RestClient.DownloadData(url);
    }

    public void Download(Attachment attachment, FileInfo fullFileName)
    {
        var url = GetRequestUrl(attachment);

        _jira.RestClient.Download(url, fullFileName.FullName);
    }
}