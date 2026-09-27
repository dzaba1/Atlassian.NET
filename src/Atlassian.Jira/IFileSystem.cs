namespace Atlassian.Jira;

/// <summary>
/// Abstracts file system access so it can be mocked in tests.
/// </summary>
public interface IFileSystem
{
    /// <summary>
    /// Reads the entire contents of the file at the given path.
    /// </summary>
    /// <param name="path">The path of the file to read.</param>
    /// <returns>The contents of the file as a byte array.</returns>
    byte[] FileReadAllBytes(string path);
}
