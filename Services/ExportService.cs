namespace PTreeGold.Services;

public class ExportService
{
    public async Task<string> ExportAsync(IReadOnlyList<string> passwords, string filePath)
    {
        // Validate path is a local file (not UNC or rooted to a network share)
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path must not be empty.");

        string fullPath = Path.GetFullPath(filePath);

        if (fullPath.StartsWith(@"\\"))
            throw new ArgumentException("Network paths are not supported. Specify a local file path.");

        string? directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        await File.WriteAllLinesAsync(fullPath, passwords);
        return fullPath;
    }
}
