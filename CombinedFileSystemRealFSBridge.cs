using System.Text;

partial class CombinedFileSystem
{
    private string BytesToString(ReadOnlySpan<byte> a)
        => Encoding.UTF8.GetString(a.ToArray());

    private byte[] StringToBytes(string a)
        => Encoding.UTF8.GetBytes(a);

    private ReadOnlySpan<byte> StringToReadOnlySpanByte(string a)
        => StringToBytes(a).AsSpan();

    private string[] GetSpecificPathActualFiles(string path)
    {
        path = path.Trim('/');

        List<string> gotEntries = new();

        foreach (var targetRoot in CombinePath)
        {
            string actualPath = Path.Combine(targetRoot, path);

            if (!Directory.Exists(actualPath))
                continue;

            gotEntries.AddRange(Directory.GetFiles(actualPath).Select(v => Path.GetFileName(v)));
        }

        return gotEntries.Distinct().ToArray();
    }

    private string[] GetSpecificPathActualDirectories(string path)
    {
        path = path.Trim('/');

        List<string> gotEntries = new();

        foreach (var targetRoot in CombinePath)
        {
            string actualPath = Path.Combine(targetRoot, path);

            if (!Directory.Exists(actualPath))
                continue;

            gotEntries.AddRange(Directory.GetDirectories(actualPath).Select(v => Path.GetFileName(v)));
        }

        return gotEntries.Distinct().ToArray();
    }


    private string[] GetSpecificPathActualEntries(ReadOnlySpan<byte> path)
    {
        string searchPath = BytesToString(path).Trim('/');

        List<string> gotEntries = new();

        gotEntries.AddRange(GetSpecificPathActualFiles(searchPath));
        gotEntries.AddRange(GetSpecificPathActualDirectories(searchPath));

        return gotEntries.Distinct().ToArray();
    }

    private bool IsActualFileFound(ReadOnlySpan<byte> path)
        => IsActualFileFound(BytesToString(path));

    private bool IsActualFileFound(string path)
    {
        path = path.Trim('/');

        foreach (var targetRoot in CombinePath)
        {
            string actualPath = Path.Combine(targetRoot, path);

            if (File.Exists(actualPath))
                return true;
        }

        return false;
    }

    private bool IsActualDirectoryFound(ReadOnlySpan<byte> path)
        => IsActualDirectoryFound(BytesToString(path));

    private bool IsActualDirectoryFound(string path)
    {
        path = path.Trim('/');

        foreach (var targetRoot in CombinePath)
        {
            string actualPath = Path.Combine(targetRoot, path);

            if (Directory.Exists(actualPath))
                return true;
        }

        return false;
    }

    private long GetActualFileSize(string path)
    {
        path = path.Trim('/');

        foreach (var targetRoot in CombinePath)
        {
            string actualPath = Path.Combine(targetRoot, path);

            if (File.Exists(actualPath))
                return new FileInfo(actualPath).Length;
        }

        return 0;
    }

    private FileStream? OpenActualFile(ReadOnlySpan<byte> path)
        => OpenActualFile(BytesToString(path));

    private FileStream? OpenActualFile(string path)
    {
        path = path.Trim('/');

        foreach (var targetRoot in CombinePath)
        {
            string actualPath = Path.Combine(targetRoot, path);

            if (File.Exists(actualPath))
                return new FileStream(actualPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        return null;
    }
}
