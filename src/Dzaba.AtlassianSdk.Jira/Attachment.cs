
using System;
using System.IO;
using Dzaba.AtlassianSdk.Jira.Services;

namespace Dzaba.AtlassianSdk.Jira;

public class Attachment
{
    private readonly IAttachmentService _attachmentService;
    
    public Attachment(Model.V3.Attachment model, IAttachmentService attachmentService)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(attachmentService);

        Model = model;
        _attachmentService = attachmentService;
    }

    public Model.V3.Attachment Model { get; }

    /// <summary>
    /// Downloads attachment as a byte array.
    /// </summary>
    public byte[] DownloadData()
    {
        return _attachmentService.DownloadData(this);
    }

    /// <summary>
    /// Downloads attachment to specified file
    /// </summary>
    /// <param name="fullFileName">Full file name where attachment will be downloaded</param>
    public void Download(FileInfo fullFileName)
    {
        ArgumentNullException.ThrowIfNull(fullFileName);

        _attachmentService.Download(this, fullFileName);
    }
}