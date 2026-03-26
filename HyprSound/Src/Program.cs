using HyprSound;

using var monitor = new HyprlandEventMonitor();
var cts = new CancellationTokenSource();

Console.CancelKeyPress += (_, eventArgs) => {
    Console.WriteLine("\n收到停止信号，正在清理资源...");
    eventArgs.Cancel = true;
    cts.Cancel();
};

try {
    await monitor.StartMonitor(cts.Token);
}
catch (Exception ex) {
    Console.WriteLine($"程序异常: {ex.Message}");
}
finally {
    Console.WriteLine("资源已释放，程序退出");
}