using System;
using System.Text;
using Tmds.Fuse;
using static Tmds.Linux.LibC;
using Tmds.Linux;

partial class CombinedFileSystem : FuseFileSystemBase
{

    public CombinedFileSystem(IEnumerable<string> combinePath)
    {
        CombinePath = combinePath.ToArray();
    }

    private static string[] CombinePath = { };

    private static readonly byte[] _helloFilePath = Encoding.UTF8.GetBytes("/hello");
    private static readonly byte[] _hello123FilePath = Encoding.UTF8.GetBytes("/hello123");
    private static readonly byte[] _helloFileContent = Encoding.UTF8.GetBytes("hello world!");

    public override int GetAttr(ReadOnlySpan<byte> path, ref stat stat, FuseFileInfoRef fiRef)
    {
        string strPath = BytesToString(path);

        if (IsActualDirectoryFound(strPath))
        {
            stat.st_mode = S_IFDIR | 0b101_101_101;
            stat.st_nlink = (ulong)(2 + GetSpecificPathActualDirectories(strPath).Length); // 2 + nr of subdirectories
            return 0;
        }
        else if (IsActualFileFound(strPath))
        {
            stat.st_mode = S_IFREG | 0b100_100_100;
            stat.st_nlink = 1;
            stat.st_size = GetActualFileSize(strPath);
            return 0;
        }
        else
        {
            return -ENOENT;
        }
    }

    private Dictionary<string, FileStream> fileStreamMap = new();

    public override int Open(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
    {
        string strPath = BytesToString(path);

        if (!IsActualFileFound(path))
            return -ENOENT;

        if ((fi.flags & O_ACCMODE) != O_RDONLY)
        {
            return -EACCES;
        }

        if (fileStreamMap.ContainsKey(strPath))
            return 0;
        var fs = OpenActualFile(strPath);
        if (fs == null)
            return -EACCES;
        fileStreamMap.Add(strPath, fs);

        return 0;
    }

    public override void Release(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
    {
        string strPath = BytesToString(path);

        if (fileStreamMap.ContainsKey(strPath))
        {
            fileStreamMap[strPath].Dispose();
            fileStreamMap.Remove(strPath);
        }
    }

    public override int Read(ReadOnlySpan<byte> path, ulong offset, Span<byte> buffer, ref FuseFileInfo fi)
    {
        string strPath = BytesToString(path);

        if (!fileStreamMap.ContainsKey(strPath))
            return -1;

        long fileSize = GetActualFileSize(strPath);

        if (offset > (ulong)fileSize)
        {
            return 0;
        }
        int intOffset = (int)offset;
        int length = (int)Math.Min(fileSize - intOffset, buffer.Length);
        byte[] bufferArray = new byte[buffer.Length];
        fileStreamMap[strPath].Seek((long)offset, SeekOrigin.Begin);
        fileStreamMap[strPath].Read(bufferArray, 0, length);
        bufferArray.CopyTo(buffer);
        return length;
    }

    public override int ReadDir(ReadOnlySpan<byte> path, ulong offset, ReadDirFlags flags, DirectoryContent content, ref FuseFileInfo fi)
    {
        if (!IsActualDirectoryFound(path))
        {
            return -ENOENT;
        }
        content.AddEntry(".");
        content.AddEntry("..");

        foreach (var entry in GetSpecificPathActualEntries(path))
            content.AddEntry(entry);

        return 0;
    }

    public override void Dispose()
    {
        foreach (var openedFiles in fileStreamMap)
        {
            openedFiles.Value.Dispose();
            fileStreamMap.Remove(openedFiles.Key);
        }

        base.Dispose();
    }

}