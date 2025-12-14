namespace VirtualFileSystem.API.Contracts;

public interface IDirectoryManager
{
    void Md(string name);
    void Cd(string name);
    void Rd(string name);
}