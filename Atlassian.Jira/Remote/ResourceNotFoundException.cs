using System;

namespace Atlassian.Jira.Remote;

/// <summary>
/// Exception thrown when the server responds with HTTP code 404.
/// </summary>
public class ResourceNotFoundException : InvalidOperationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceNotFoundException"/> class.
    /// </summary>
    public ResourceNotFoundException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceNotFoundException"/> class with the given message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ResourceNotFoundException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceNotFoundException"/> class with the given message and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that caused the current exception.</param>
    public ResourceNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
