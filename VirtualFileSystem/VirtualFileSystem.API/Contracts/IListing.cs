namespace VirtualFileSystem.API.Contracts;

public interface IListing
{
    FileEntryInfo[] Ls();
}