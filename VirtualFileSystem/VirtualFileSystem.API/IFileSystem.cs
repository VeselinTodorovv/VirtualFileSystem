namespace VirtualFileSystem.API;

public interface IFileSystem
{
    void Cpin(string externalPath, string internalName);
    void Cpout(string internalName, string destinationPath);
    void Rm(string internalName);
    FileEntryInfo[] Ls();
}