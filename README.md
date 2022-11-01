# CombineFS
Combine 2 or more directory into 1 fuse file system

This application only works with Linux (fuse)

```mermaid
graph LR;
  FUSE -- a/b.txt<br>c/d.txt --> CombineFS;
  CombineFS -- a/b.txt --> Directory1;
  CombineFS -- c/d.txt --> Directory2;
```

## Usage

Currently, you have to edit `Program.cs` to change mount point and directory to combine files.
