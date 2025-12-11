using VirtualFileSystem.API;

namespace VirtualFileSystem.CLI.Commands.Files;

public class LsCommand : ICommand
{
    public string Name => "ls";
    public string Description => "List files in the container.";
    public string Usage => "ls";
    
    public void Execute(IFileSystem fs, string[] args)
    {
        var list = fs.Ls();
        
        if (list.Length == 0)
        {
            Console.WriteLine("(empty)");
            return;
        }
        
        Console.WriteLine("Name".PadRight(20) + "Size".PadRight(10));
        Console.WriteLine(new string('-', 30));
        
        foreach (var file in list)
        {
            string sizeStr = file.IsDir ? "" : $"{file.Size} bytes";
            
            Console.WriteLine($"{file.Name,-20}{sizeStr,-10}");
            Console.WriteLine();
        }
    }
}