using VirtualFileSystem.API;
using VirtualFileSystem.API.Contracts;
using VirtualFileSystem.CLI.Commands;

namespace VirtualFileSystem.CLI;

internal static class Program
{
    private static readonly SimpleDictionary<string, ICommand> Commands;
    
    static Program()
    {
        Commands = new SimpleDictionary<string, ICommand>(4);
        
        Commands.Add("cpin", new CpinCommand());
        Commands.Add("cpout", new CpoutCommand());
        Commands.Add("ls", new LsCommand());
        Commands.Add("rm", new RmCommand());
    }
    
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintUsage();
            return;
        }
        
        const string containerPath = "container.vfs";
        const int blockSize = 4096;
        
        bool debug = false;
        
        if (args[0] == "--debug")
        {
            debug = true;
            
            // TODO: Prolly not allowed
            args = args.Skip(1).ToArray();
        }
        
        if (!Commands.TryGetValue(args[0], out var command))
        {
            Console.WriteLine($"Unknown command: {args[0]}");
            PrintUsage();
            
            return;
        }
        
        IFileSystem fs = new FileSystemService(containerPath, blockSize);
        
        if (debug)
        {
            fs.SetDebug(true);
        }
        
        command?.Execute(fs, args);
    }
    
    private static void PrintUsage()
    {
        Console.WriteLine("Available commands:\n");
        
        foreach (var c in Commands.Values())
        {
            Console.WriteLine($" {c.Usage,-30} {c.Description}");
        }
    }
}