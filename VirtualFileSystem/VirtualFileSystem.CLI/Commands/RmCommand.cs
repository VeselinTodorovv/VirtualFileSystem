using VirtualFileSystem.API;

namespace VirtualFileSystem.CLI.Commands;

public class RmCommand : ICommand
{
    public string Name => "rm";
    public string Description => "Remove a file from the virtual FS.";
    public string Usage => "rm <internal-name>";
    
    public void Execute(IFileSystem fs, string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine($"Usage: {Usage}");
            return;
        }
        
        try
        {
            fs.Rm(args[1]);
            Console.WriteLine($"[OK] Removed '{args[1]}'.");
        }
        catch (Exception e)
        {
            Console.WriteLine($"[ERROR] {e.Message}");
        }
    }
}