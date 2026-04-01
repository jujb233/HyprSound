using HyprSound.Map;
using HyprSound.Monitor;
using HyprSound.Player;
using HyprSound.Resolve;
using HyprSound.Type;
using HyprSound.Type.Hypr;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var commandLineArgs = args.Length == 1 && !args[0].StartsWith('-')
    ? ["-l", args[0]]
    : args;

var configuration = new ConfigurationBuilder()
    .AddCommandLine(commandLineArgs, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
        ["-h"] = "help",
        ["--help"] = "help",
        ["-p"] = "path",
        ["--path"] = "path",
        ["-l"] = "library",
        ["--library"] = "library"
    })
    .Build();

if (commandLineArgs.Contains("-h") || commandLineArgs.Contains("--help") || configuration["help"] == "true") {
    ShowHelp();
    return;
}

var soundLibrary = configuration["library"];
var assetPath = configuration["path"] ?? Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
    ".config", "hyprsound");

if (soundLibrary is null or "" or " ") {
    Console.WriteLine("错误：音效库名不能为空。");
    ShowHelp();
    return;
}

if (!Directory.Exists(assetPath)) {
    Console.WriteLine($"错误：Asset 目录不存在: {assetPath}");
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
    Console.WriteLine("  <音效库名>          要使用的音效库目录名（等同于 -l <音效库名>）。");
    Console.WriteLine();
    Console.WriteLine("选项:");
    Console.WriteLine("  -p, --path <目录>   指定 Asset 根目录路径（默认为 ~/.config/hyprsound/）");
    Console.WriteLine("  -l, --library <名称> 指定音效库名称（也可直接作为位置参数传入）");
    Console.WriteLine("  -h, --help          显示此帮助信息");
    Console.WriteLine();
    Console.WriteLine("示例:");
    Console.WriteLine("  hyprsound default                      # 使用 ~/.config/hyprsound/default 音效库");
    Console.WriteLine("  hyprsound -p /path/to/asset -l library      # 使用指定目录下的 library 音效库");
    Console.WriteLine("  hyprsound --path /path/to/asset --library library      # 使用显式参数指定音效库");
}