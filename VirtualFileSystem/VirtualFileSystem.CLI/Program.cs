using VirtualFileSystem.API;
using VirtualFileSystem.CLI.Commands.Directories;
using VirtualFileSystem.CLI.Commands.Files;

namespace VirtualFileSystem.CLI;

internal static class Program
{
    private static readonly SimpleDictionary<string, ICommand> Commands;
    
    static Program()
    {
        Commands = new SimpleDictionary<string, ICommand>(7);
        
        Commands.Add("cpin", new CpinCommand());
        Commands.Add("cpout", new CpoutCommand());
        Commands.Add("ls", new LsCommand());
        Commands.Add("rm", new RmCommand());
        
        Commands.Add("md", new MdCommand());
        Commands.Add("cd", new CdCommand());
        Commands.Add("rd", new RdCommand());
    }
    
    public static void Main(string[] args)
    {
        const string containerPath = "container.vfs";
        const int blockSize = 4096;
        
        bool debug = args is ["--debug"];
        
        IFileSystem fs = new FileSystemService(containerPath, blockSize);
        
        if (debug)
        {
            fs.SetDebug(true);
        }
        
        Console.WriteLine("Virtual File System");
        Console.WriteLine("Type 'exit' to quit.\n");
        
        while (true)
        {
            Console.Write("VFS> ");
            string? input = Console.ReadLine();
            
            if (input == "")
            {
                continue;
            }
            
            if (input is "exit")
            {
                break;
            }
            
            var tokens = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            if (tokens.Length == 0)
            {
                continue;
            }
            
            if (!Commands.TryGetValue(tokens[0], out var command))
            {
                Console.WriteLine($"[ERROR] Unknown command: {tokens[0]}");
                PrintUsage();
                
                continue;
            }
            
            try
            {
                command?.Execute(fs, tokens);
            }
            catch (Exception e)
            {
                Console.WriteLine($"[ERROR] {e.Message}");
            }
        }
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