using VirtualFileSystem.API;

namespace VirtualFileSystem.CLI.Commands.Directories;

public class RdCommand : ICommand
{
    public string Description => "Remove a directory from the virtual FS, along with any subfolders and files inside it.";
    public string Usage => "rd <internal-name>";
    
    public void Execute(IFileSystem fs, string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine($"Usage: {Usage}");
            return;
        }
        
        try
        {
            fs.Rd(args[1]);
            Console.WriteLine($"[OK] Removed '{args[1]}'.\n");
        }
        catch (Exception e)
        {
            Console.WriteLine($"[ERROR] {e.Message}\n");
        }
    }
}