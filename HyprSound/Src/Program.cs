using System.CommandLine;
using HyprSound.Map;
using HyprSound.Monitor;
using HyprSound.Player;
using HyprSound.Resolve;
using HyprSound.Type;
using HyprSound.Type.Hypr;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var libraryArgument = new Argument<string?>("library") {
    Description = "要使用的音效库目录名"
};

var libraryOption = new Option<string?>("--library") {
    Description = "指定音效库名称"
};
libraryOption.Aliases.Add("-l");

var pathOption = new Option<string?>("--path") {
    Description = "指定 Asset 根目录路径（默认为 ~/.config/hyprsound/）"
};
pathOption.Aliases.Add("-p");

var rootCommand = new RootCommand("HyprSound - 播放音效");
rootCommand.Arguments.Add(libraryArgument);
rootCommand.Options.Add(libraryOption);
rootCommand.Options.Add(pathOption);

rootCommand.SetAction(async parseResult => {
    var soundLibrary = parseResult.GetValue(libraryOption) ?? parseResult.GetValue(libraryArgument);

    var assetPath = parseResult.GetValue(pathOption) ?? Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".config", "hyprsound");

    if (string.IsNullOrWhiteSpace(soundLibrary)) {
        Console.WriteLine("错误：音效库名不能为空。");
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
});

return rootCommand.Parse(args).Invoke();