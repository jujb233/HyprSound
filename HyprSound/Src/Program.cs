using HyprSound;
using HyprSound.Map;
using HyprSound.Player;
using HyprSound.Type;
using Microsoft.Extensions.DependencyInjection;

if (args.Length < 2) {
    Console.WriteLine("错误：必须提供 Asset 目录路径和音效库名两个参数.\n用法 path/to/exe AssetPath SoundLibrary");
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
    .AddSingleton<ISoundMappingResolve>(_ => new JsonMappingResolve(assetPath, soundLibrary))
    .AddSingleton<IPlayer>(sp => new SdlPlayer(sp.GetRequiredService<ISoundMappingResolve>()))
    .AddSingleton<HyprlandEventMonitor>()
    .BuildServiceProvider();

var cts = new CancellationTokenSource();

Console.CancelKeyPress += (_, eventArgs) => {
    Console.WriteLine("\n收到停止信号，正在清理资源...");
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
    Console.WriteLine($"程序异常: {ex.Message}");
}
finally {
    if (serviceProvider is IDisposable disposable)
        disposable.Dispose();

    Console.WriteLine("资源已释放，程序退出");
}