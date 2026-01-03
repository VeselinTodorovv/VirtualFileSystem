using VirtualFileSystem.API;

namespace VirtualFileSystem.CLI.Commands.Directories;

public class CdCommand : ICommand
{
    public string Description => "Change the path in the virtual FS.";
    public string Usage => "cd <argument>";
    
    public void Execute(IFileSystem fs, string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine($"Usage: {Usage}");
            return;
        }
        
        try
        {
            fs.Cd(args[1]);
            
            Console.WriteLine("[OK] Path changed.\n");
        }
        catch (Exception e)
        {
            Console.WriteLine($"[ERROR] {e.Message}\n");
        }
    }
}