// See https://aka.ms/new-console-template for more information
using VirtualFileSystem.API;

namespace VirtualFileSystem.CLI;

internal static class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintUsage();
            return;
        }
        
        var command = args[0];
        
        const string containerPath = "container.vfs"; // adjustable
        const int blockSize = 4096;
        var fs = new FileSystemService(containerPath, blockSize);
        
        switch (command)
        {
            case "cpin":
                if (args.Length != 3)
                {
                    Console.WriteLine("Usage: cpin <external-path> <internal-name>");
                    return;
                }
                
                fs.Cpin(args[1], args[2]);
                
                break;
            
            case "cpout":
                if (args.Length != 3)
                {
                    Console.WriteLine("Usage: cpout <internal-name> <external-path>");
                    return;
                }
                
                fs.Cpout(args[1], args[2]);
                
                break;
            
            case "ls":
                var list = fs.Ls();
                foreach (var entry in list)
                {
                    Console.WriteLine($"{entry.Name} ({entry.Size} bytes)");
                }
                
                break;
            
            case "rm":
                if (args.Length != 2)
                {
                    Console.WriteLine("Usage: rm <internal-name>");
                    return;
                }
                
                fs.Rm(args[1]);
                
                break;
            
            default:
                Console.WriteLine("Unknown command.");
                PrintUsage();
                
                break;
        }
    }
    
    private static void PrintUsage()
    {
        Console.WriteLine("Commands:");
        Console.WriteLine(" cpin <external> <internal>");
        Console.WriteLine(" cpout <internal> <external>");
        Console.WriteLine(" ls");
        Console.WriteLine(" rm <internal>");
    }
}