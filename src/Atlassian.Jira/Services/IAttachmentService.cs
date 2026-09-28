using System.IO;

namespace Atlassian.Jira.Services;

public interface IAttachmentService
{
    /// <summary>
    /// Downloads attachment as a byte array.
    /// </summary>
    byte[] DownloadData(Attachment attachment);

    /// <summary>
    /// Downloads attachment to specified file
    /// </summary>
    /// <param name="attachment"></param>
    /// <param name="fullFileName">Full file name where attachment will be downloaded</param>
    void Download(Attachment attachment, FileInfo fullFileName);
}