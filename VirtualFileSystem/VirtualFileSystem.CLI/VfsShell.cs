using VirtualFileSystem.API;
using VirtualFileSystem.CLI.Commands.Directories;
using VirtualFileSystem.CLI.Commands.Files;

namespace VirtualFileSystem.CLI;

public class VfsShell
{
    private readonly IFileSystem _fileSystem;
    private readonly LinearMap<string, ICommand> _commands;
    
    public VfsShell(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
        _commands = new LinearMap<string, ICommand>(7);
        InitializeCommands();
    }
    
    private void InitializeCommands()
    {
        _commands.Add("cpin", new CpinCommand());
        _commands.Add("cpout", new CpoutCommand());
        _commands.Add("ls", new LsCommand());
        _commands.Add("rm", new RmCommand());
        _commands.Add("md", new MdCommand());
        _commands.Add("cd", new CdCommand());
        _commands.Add("rd", new RdCommand());
    }
    
    public void Run(string[] args)
    {
        bool debug = args is ["--debug"];
        
        if (debug)
        {
            _fileSystem.SetDebug(true);
        }
        
        Console.WriteLine("Virtual File System");
        Console.WriteLine("Type 'exit' to quit.\n");
        
        while (true)
        {
            Console.Write("VFS> ");
            string? input = Console.ReadLine();
            
            if (input is "exit")
            {
                break;
            }
            
            var tokens = Tokenize(input);
            
            if (tokens.Count == 0)
            {
                continue;
            }
            
            if (!_commands.TryGetValue(tokens[0], out var command))
            {
                Console.WriteLine($"[ERROR] Unknown command: {tokens[0]}");
                PrintUsage();
                
                continue;
            }
            
            command?.Execute(_fileSystem, tokens.ToArray());
        }
    }
    
    private static List<string> Tokenize(string? input)
    {
        List<string> tokens = [];
        int i = 0;
        
        while (input != null && i < input.Length)
        {
            // Skip leading whitespace
            while (i < input.Length && input[i] == ' ')
            {
                i++;
            }
            
            if (i >= input.Length)
            {
                break;
            }
            
            // Start of token
            int start = i;
            
            while (i < input.Length && input[i] != ' ')
            {
                i++;
            }
            
            int length = i - start;
            if (length <= 0)
            {
                continue;
            }
            
            string token = "";
            for (int j = 0; j < length; j++)
            {
                token += input[start + j];
            }
            
            tokens.Add(token);
        }
        
        return tokens;
    }
    
    private void PrintUsage()
    {
        Console.WriteLine("Available commands:\n");
        
        foreach (var c in _commands.Values())
        {
            Console.WriteLine($" {c.Usage,-30} {c.Description}");
        }
    }
}