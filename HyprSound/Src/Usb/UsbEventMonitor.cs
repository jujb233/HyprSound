using HyprSound.Event;
using HyprSound.Interface;
using Microsoft.Extensions.Logging;
using Usb.Events;

namespace HyprSound.Usb;

public class UsbEventMonitor(ILogger<UsbEventMonitor> logger) : IMonitor {
    private readonly CancellationTokenSource _cts = new();

    private readonly UsbEventWatcher _watcher = new(
        startImmediately: true,
        addAlreadyPresentDevicesToList: false,
        usePnPEntity: false,
        includeTTY: false
    );

    public event Action<IEventType>? EventOccurred;

    public async Task StartAsync(CancellationToken externalToken = default) {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(externalToken, _cts.Token);
        var token = linkedCts.Token;

        _watcher.UsbDriveMounted += OnUsbDriveMounted;
        _watcher.UsbDriveEjected += OnUsbDriveEjected;

        logger.LogInformation("USB 事件监控已启动");

        try {
            await Task.Delay(-1, token);
        }
        catch (OperationCanceledException) {
            logger.LogInformation("USB 事件监控已停止");
        }
        catch (Exception ex) {
            logger.LogError(ex, "USB 事件监控发生异常");
        }
        finally {
            DisposeWatcher();
        }
    }

    private void OnUsbDriveEjected(object? sender, string path) {
        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug("USB 驱动器已卸载: {Path}", path);

        EventOccurred?.Invoke(new StandardEvent("UsbDevice", EventKind.UsbDriveEjected));
    }

    private void OnUsbDriveMounted(object? sender, string path) {
        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug("USB 驱动器已挂载: {Path}", path);

        EventOccurred?.Invoke(new StandardEvent("UsbDevice", EventKind.UsbDriveMounted));
    }

    private void DisposeWatcher() {
        _watcher.UsbDriveMounted -= OnUsbDriveMounted;
        _watcher.UsbDriveEjected -= OnUsbDriveEjected;
        _watcher.Dispose();
    }

    public void Dispose() {
        DisposeWatcher();
        _cts.Cancel();
        _cts.Dispose();
        GC.SuppressFinalize(this);
    }
}