using HyprSound;
using HyprSound.Map;
using HyprSound.Player;
using HyprSound.Type;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

switch (args.Length) {
    case 1 when (args[0] == "-h" || args[0] == "--help"):
        ShowHelp();
        return;
    case < 2:
        Console.WriteLine("错误：必须提供 Asset 目录路径和音效库名两个参数。");
        ShowHelp();
        return;
}

var assetPath = args[0];
var soundLibrary = args[1];

if (!Directory.Exists(assetPath)) {
    Console.WriteLine($"错误：Asset 目录不存在: {assetPath}");
    return;
}

if (soundLibrary is null or "" or " ") {
    Console.WriteLine("错误：未指定子目录(音效库)名");
    return;
}

var serviceProvider = new ServiceCollection()
    .AddLogging(builder => {
        builder.AddConsole();
        builder.SetMinimumLevel(LogLevel.Information);
    })
    .AddSingleton<ISoundMappingResolve>(sp =>
        new JsonMappingResolve(assetPath, soundLibrary, sp.GetRequiredService<ILogger<JsonMappingResolve>>())
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
    Console.WriteLine("用法: 程序名 <Asset目录路径> <音效库名>");
    Console.WriteLine();
    Console.WriteLine("参数说明:");
    Console.WriteLine("  Asset目录路径    音效库(复数)资源集合所在的目录路径");
    Console.WriteLine("  音效库名         音效库目录名（例如 \"default\"）");
    Console.WriteLine();
    Console.WriteLine("选项:");
    Console.WriteLine("  -h, --help       显示此帮助信息");
}