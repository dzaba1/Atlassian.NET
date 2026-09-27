namespace Atlassian.Jira;

/// <summary>
/// Information about an attachment to be uploaded
/// </summary>
public class UploadAttachmentInfo
{
    /// <summary>
    /// Gets or sets the file name of the attachment.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the binary contents of the attachment.
    /// </summary>
    public byte[] Data { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UploadAttachmentInfo"/> class.
    /// </summary>
    /// <param name="name">The file name of the attachment.</param>
    /// <param name="data">The binary contents of the attachment.</param>
    public UploadAttachmentInfo(string name, byte[] data)
    {
        Name = name;
        Data = data;
    }
}
