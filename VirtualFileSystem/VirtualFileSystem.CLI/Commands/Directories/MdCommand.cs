using VirtualFileSystem.API;

namespace VirtualFileSystem.CLI.Commands.Directories;

public class MdCommand : ICommand
{
    public string Name => "md";
    public string Description => "Remove a directory from the virtual FS.";
    public string Usage => "md <internal-name>";
    
    public void Execute(IFileSystem fs, string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine($"Usage: {Usage}");
            return;
        }

        try
        {
            fs.Mkdir(args[1]);
            Console.WriteLine($"[OK] Created '{args[1]}'.");
        }
        catch (Exception e)
        {
            Console.WriteLine($"[ERROR] {e.Message}");
        }
    }
}