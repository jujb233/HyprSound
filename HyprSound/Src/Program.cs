using HyprSound;
using HyprSound.Map;
using HyprSound.Player;
using HyprSound.Type;
using Microsoft.Extensions.DependencyInjection;

var assetPath = Path.Combine(AppContext.BaseDirectory, "..", "Asset");
var mappingConfigPath = Path.Combine(assetPath, "sound-mapping.json");

var serviceProvider = new ServiceCollection()
    .AddSingleton<ISoundMappingResolve>(_ => new JsonMappingResolve(mappingConfigPath, assetPath))
    .AddSingleton<IPlayer, SdlPlayer>()
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