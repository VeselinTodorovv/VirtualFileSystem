namespace VirtualFileSystem.API.Contracts;

public interface IImporter
{
    void Cpin(string externalPath, string internalName);
}