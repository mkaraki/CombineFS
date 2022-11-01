using Tmds.Fuse;

if (args.Length < 2)
{
    Console.Error.WriteLine("CombineFS.exe {Combine Directories}[..] {Mount Point}");
    Environment.Exit(1);
    return;
}

if (!Fuse.CheckDependencies())
{
    Console.WriteLine(Fuse.InstallationInstructions);
    Environment.Exit(2);
    return;
}

string[] combineDirs = args.Take(args.Length - 1).ToArray();
string mountPoint = args.Last();

if (!Directory.Exists(mountPoint))
{
    Console.Error.WriteLine("No mountpoint found");
    Environment.Exit(3);
    return;
}

using (var mount = Fuse.Mount(mountPoint, new CombinedFileSystem(combineDirs)))
{
    await mount.WaitForUnmountAsync();
}