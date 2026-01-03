using VirtualFileSystem.API;

namespace VirtualFileSystem.CLI.Commands.Files;

public class CpinCommand : ICommand
{
    public string Description => "Copy an external file into the virtual FS.";
    public string Usage => "cpin <external-path> <internal-name>";
    
    public void Execute(IFileSystem fs, string[] args)
    {
        if (args.Length != 3)
        {
            Console.WriteLine($"Usage: {Usage}");
            return;
        }
        
        try
        {
            fs.Cpin(args[1], args[2]);
            Console.WriteLine($"[OK] Imported '{args[1]}' as '{args[2]}'.\n");
        }
        catch (Exception e)
        {
            Console.WriteLine($"[ERROR] {e.Message}\n");
        }
    }
}