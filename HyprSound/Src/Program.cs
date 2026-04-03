using System.CommandLine;
using HyprSound;
using HyprSound.Hyprland;
using HyprSound.Hyprland.Event;
using HyprSound.Interface;
using HyprSound.MappingResolve;
using HyprSound.Player;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var logLevelOption = new Option<LogLevel?>("--log-level", "-v") {
    Description = "设置日志级别 (None, Error, Warning, Information, Debug, Trace)",
    HelpName = "level",
    Arity = ArgumentArity.ZeroOrOne,
    Validators = {
        result => {
            var value = result.Tokens.SingleOrDefault()?.Value;
            if (string.IsNullOrEmpty(value))
                return;

            if (Enum.TryParse<LogLevel>(value, true, out var _))
                return;

            result.AddError($"无效的日志级别: {value}。有效值: None, Error, Warning, Information, Debug, Trace");
        }
    }
};

var libraryOption = new Option<string>("--library", "-l") {
    Description = "指定音效库名称（必需）",
    Required = true,
    Validators = {
        result => {
            if (string.IsNullOrWhiteSpace(result.GetValueOrDefault<string>()))
                result.AddError("音效库名不能为空。");
        }
    }
};

var pathOption = new Option<string>("--path", "-p") {
    Description = "指定 Asset 根目录路径（默认为 ~/.config/hyprsound/）",
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
rootCommand.Options.Add(logLevelOption);

initCommand.Options.Add(libraryOption);
initCommand.Options.Add(pathOption);
initCommand.Options.Add(logLevelOption);


initCommand.SetAction(result => {
    var soundLibrary = result.GetRequiredValue(libraryOption);
    var assetPath = GetAssetPath(result.GetValue(pathOption));
    var libraryPath = Path.Combine(assetPath, soundLibrary);

    var logLevel = result.GetValue(logLevelOption) ?? LogLevel.Information;
    var serviceProvider = new ServiceCollection()
        .AddLogging(builder => {
            builder.AddConsole();
            builder.SetMinimumLevel(logLevel);
        })
        .AddSingleton<IEventCatalog, HyprlandEventCatalog>()
        .AddSingleton<Initialization>()
        .BuildServiceProvider();

    var catalogs = serviceProvider.GetService<IEventCatalog>();
    if (catalogs is null) {
        Console.WriteLine("错误: 事件列表为空,无法初始化配置文件.");
        return;
    }

    var eventNames = (new[] { catalogs })
        .SelectMany(static catalog => catalog.EventNames)
        .Where(static name => name is not ("" or " "))
        .ToHashSet(StringComparer.Ordinal); // TODO 事件重名处理

    var initialization = serviceProvider.GetRequiredService<Initialization>();
    initialization.InitJsonFile(libraryPath, eventNames);
});
rootCommand.Add(initCommand);


rootCommand.SetAction(async result => {
    var soundLibrary = result.GetRequiredValue(libraryOption);
    var assetPath = GetAssetPath(result.GetValue(pathOption));

    var logLevel = result.GetValue(logLevelOption) ?? LogLevel.Information;
    var serviceProvider = new ServiceCollection()
        .AddLogging(builder => {
            builder.AddConsole();
            builder.SetMinimumLevel(logLevel);
        })
        .AddSingleton<IEventCatalog, HyprlandEventCatalog>()
        .AddSingleton<IEventParser, HyprlandEventResolve>()
        .AddSingleton<ISoundMappingResolve>(provider =>
            new JsonMappingResolve(
                assetPath,
                soundLibrary,
                provider.GetServices<IEventCatalog>(),
                provider.GetRequiredService<ILogger<JsonMappingResolve>>()
            )
        )
        .AddSingleton<IPlayer>(provider =>
            new SdlPlayer(provider.GetRequiredService<ISoundMappingResolve>(),
                provider.GetRequiredService<ILogger<SdlPlayer>>())
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