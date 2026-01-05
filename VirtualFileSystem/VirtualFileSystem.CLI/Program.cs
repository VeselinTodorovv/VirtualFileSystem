using Microsoft.Extensions.DependencyInjection;
using VirtualFileSystem.API;

namespace VirtualFileSystem.CLI;

internal static class Program
{
    public static void Main(string[] args)
    {
        var serviceProvider = ConfigureServices();
        
        var shell = serviceProvider.GetRequiredService<VfsShell>();
        shell.Run(args);
    }
    
    private static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        
        const string containerPath = "container.vfs";
        const int blockSize = 2048;
        
        services.AddScoped<IFileSystem>(_ => new FileSystemService(containerPath, blockSize));
        services.AddTransient<VfsShell>();
        
        return services.BuildServiceProvider();
    }
}