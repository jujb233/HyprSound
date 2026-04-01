using HyprSound.Map;
using HyprSound.Monitor;
using HyprSound.Player;
using HyprSound.Resolve;
using HyprSound.Type;
using HyprSound.Type.Hypr;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

string assetPath;
string soundLibrary;

switch (args.Length) {
    case 0:
    case 1 when (args[0] == "-h" || args[0] == "--help"):
        ShowHelp();
        return;
    // 处理 -p / --path 后参数不足的情况
    case 1 when (args[0] == "-p" || args[0] == "--path"):
        Console.WriteLine("错误：使用 -p 选项时需要指定 Asset 目录和音效库名。");
        ShowHelp();
        return;
    case 1: {
        // 用法：可执行文件 library-name
        soundLibrary = args[0];
        assetPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".config", "hyprsound");

        if (!Directory.Exists(assetPath)) {
            Console.WriteLine($"错误：默认 Asset 目录不存在: {assetPath}");
            Console.WriteLine("请使用 -p 选项指定 Asset 目录路径。");
            return;
        }

        break;
    }
    case 3 when (args[0] == "-p" || args[0] == "--path"): {
        // 用法：可执行文件 -p /path/to/some library-name
        assetPath = args[1];
        soundLibrary = args[2];

        if (!Directory.Exists(assetPath)) {
            Console.WriteLine($"错误：指定的 Asset 目录不存在: {assetPath}");
            return;
        }

        break;
    }
    default:
        Console.WriteLine("错误：参数格式不正确。");
        ShowHelp();
        return;
}


if (string.IsNullOrWhiteSpace(soundLibrary)) {
    Console.WriteLine("错误：音效库名不能为空。");
    return;
}

// 检查音效库子目录是否存在
var libraryPath = Path.Combine(assetPath, soundLibrary);
if (!Directory.Exists(libraryPath)) {
    Console.WriteLine($"错误：音效库目录不存在: {libraryPath}");
    return;
}

var serviceProvider = new ServiceCollection()
    .AddLogging(builder => {
        builder.AddConsole();
        builder.SetMinimumLevel(LogLevel.Information);
    })
    .AddSingleton<IEventCatalog, HyprlandEventCatalog>()
    .AddSingleton<IEventParser, HyprlandEventResolve>()
    .AddSingleton<ISoundMappingResolve>(sp =>
        new JsonMappingResolve(
            assetPath,
            soundLibrary,
            sp.GetServices<IEventCatalog>(),
            sp.GetRequiredService<ILogger<JsonMappingResolve>>()
        )
    )
    .AddSingleton<IPlayer>(sp =>
        new SdlPlayer(sp.GetRequiredService<ISoundMappingResolve>(),
            sp.GetRequiredService<ILogger<SdlPlayer>>())
    )
    .AddSingleton<HyprlandEventMonitor>()
    .BuildServiceProvider();

var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

var cts = new CancellationTokenSource();

Console.CancelKeyPress += (_, eventArgs) => {
    logger.LogInformation("\n收到停止信号，正在清理资源...");
    eventArgs.Cancel = true;
    cts.Cancel();
};

var monitor = serviceProvider.GetRequiredService<HyprlandEventMonitor>();
var player = serviceProvider.GetRequiredService<IPlayer>();

monitor.HyprEvent += player.Play;

try {
    await monitor.StartMonitor(cts.Token);
}
catch (Exception ex) {
    logger.LogError("程序异常: {ExMessage}", ex.Message);
}
finally {
    serviceProvider.Dispose();
    Console.WriteLine("资源已释放，程序退出");
}

return;

void ShowHelp() {
    Console.WriteLine("用法: hyprsound [选项] <音效库名>");
    Console.WriteLine();
    Console.WriteLine("参数说明:");
    Console.WriteLine("  <音效库名>          要使用的音效库目录名。");
    Console.WriteLine();
    Console.WriteLine("选项:");
    Console.WriteLine("  -p, --path <目录>   指定 Asset 根目录路径（默认为 ~/.config/hyprsound/）");
    Console.WriteLine("  -h, --help          显示此帮助信息");
    Console.WriteLine();
    Console.WriteLine("示例:");
    Console.WriteLine("  hyprsound default                      # 使用 ~/.config/hyprsound/default 音效库");
    Console.WriteLine("  hyprsound -p /path/to/asset library      # 使用指定目录下的 library 音效库");
}