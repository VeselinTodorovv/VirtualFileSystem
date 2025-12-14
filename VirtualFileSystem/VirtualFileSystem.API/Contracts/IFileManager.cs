namespace VirtualFileSystem.API.Contracts;

public interface IFileManager
{
    void Cpin(string externalPath, string internalName);
    void Cpout(string name, string dest);
    void Rm(string name);
}