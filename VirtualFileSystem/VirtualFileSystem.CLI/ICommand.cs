using VirtualFileSystem.API;

namespace VirtualFileSystem.CLI;

public interface ICommand
{
    string Name { get; }
    string Description { get; }
    string Usage { get; }
    
    void Execute(IFileSystem fs, string[] args);
}