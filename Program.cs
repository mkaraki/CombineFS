using Tmds.Fuse;

if (!Fuse.CheckDependencies())
{
    Console.WriteLine(Fuse.InstallationInstructions);
    return;
}
using (var mount = Fuse.Mount("/tmp/mountpoint", new CombinedFileSystem(new string[] { "/usr/bin", "/bin", "/home/araki-mk" })))
{
    await mount.WaitForUnmountAsync();
}