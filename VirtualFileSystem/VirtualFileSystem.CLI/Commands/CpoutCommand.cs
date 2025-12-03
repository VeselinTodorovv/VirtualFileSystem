using VirtualFileSystem.API;

namespace VirtualFileSystem.CLI.Commands;

public class CpoutCommand : ICommand
{
    public string Name => "cpout";
    public string Description => "Extract a file from the virtual FS.";
    public string Usage => "cpout <internal-name> <external-path>";
    
    public void Execute(FileSystemService fs, string[] args)
    {
        if (args.Length != 3)
        {
            Console.WriteLine($"Usage: {Usage}");
            return;
        }
        
        try
        {
            fs.Cpout(args[1], args[2]);
            Console.WriteLine($"[OK] Extracted '{args[1]}' to '{args[2]}'.");
        }
        catch (Exception e)
        {
            Console.WriteLine($"[ERROR] {e.Message}");
        }
    }
}