using System.CommandLine;
using HyprSound.Map;
using HyprSound.Monitor;
using HyprSound.Player;
using HyprSound.Resolve;
using HyprSound.Type;
using HyprSound.Type.Hypr;
using HyprSound.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var libraryOption = new Option<string>("--library") {
    Description = "指定音效库名称（必需）",
    Required = true,
    Aliases = { "-l" },
    Validators = {
        result => {
            if (string.IsNullOrWhiteSpace(result.GetValueOrDefault<string>()))
                result.AddError("音效库名不能为空。");
        }
    }
};

var pathOption = new Option<string>("--path") {
    Description = "指定 Asset 根目录路径（默认为 ~/.config/hyprsound/）",
    Aliases = { "-p" },
    Validators = {
        result => {
            var path = result.GetValueOrDefault<string>();
            if (!Directory.Exists(path))
                result.AddError($"Asset 目录不存在: {path}");
        }
    }
};

var rootCommand = new RootCommand("HyprSound - 为你的使用Hyprland的Linux系统添加系统音效的命令行工具") {
    Validators = {
        result => {
            var soundLibrary = result.GetValue(libraryOption);
            var assetPath = GetAssetPath(result.GetValue(pathOption));

            if (soundLibrary is null) return;
            var libraryPath = Path.Combine(assetPath, soundLibrary);
            if (!Directory.Exists(libraryPath))
                result.AddError($"音效库目录不存在: {libraryPath}");
        }
    }
};
var initCommand = new Command("init", "初始化音效库配置文件") {
    Validators = {
        result => {
            var soundLibrary = result.GetValue(libraryOption);
            var assetPath = GetAssetPath(result.GetValue(pathOption));

            if (soundLibrary is null) return;
            var libraryPath = Path.Combine(assetPath, soundLibrary);
            if (!Directory.Exists(libraryPath))
                result.AddError($"音效库目录不存在: {libraryPath}");
        }
    }
};

rootCommand.Options.Add(libraryOption);
rootCommand.Options.Add(pathOption);

initCommand.Options.Add(libraryOption);
initCommand.Options.Add(pathOption);

initCommand.SetAction(result => {
    var soundLibrary = result.GetRequiredValue(libraryOption);
    var assetPath = GetAssetPath(result.GetValue(pathOption));
    var libraryPath = Path.Combine(assetPath, soundLibrary);

    (new Initialization()).InitJsonFile(libraryPath);
});

rootCommand.Add(initCommand);
rootCommand.SetAction(async result => {
    var soundLibrary = result.GetRequiredValue(libraryOption);
    var assetPath = GetAssetPath(result.GetValue(pathOption));

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

string GetAssetPath(string? input) =>
    input ?? Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".config", "hyprsound");