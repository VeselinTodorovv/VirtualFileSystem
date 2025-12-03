using VirtualFileSystem.API;
using VirtualFileSystem.CLI.Commands;

namespace VirtualFileSystem.CLI;

internal static class Program
{
    // TODO: Implement Own Dictionary
    private static readonly Dictionary<string, ICommand> Commands = new()
    {
        ["cpin"] = new CpinCommand(),
        ["cpout"] = new CpoutCommand(),
        ["ls"] = new LsCommand(),
        ["rm"] = new RmCommand(),
    };
    
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintUsage();
            return;
        }
        
        var cmd = args[0];
        
        if (!Commands.TryGetValue(cmd, out var command))
        {
            Console.WriteLine($"Unknown command: {cmd}");
            PrintUsage();
            return;
        }
        
        const string containerPath = "container.vfs";
        const int blockSize = 4096;
        
        var fs = new FileSystemService(containerPath,  blockSize);
        
        command.Execute(fs, args);
    }
    
    private static void PrintUsage()
    {
        Console.WriteLine("Available commands:\n");
        
        foreach (var c in Commands.Values)
        {
            Console.WriteLine($" {c.Usage,-30} {c.Description}");
        }
    }
}