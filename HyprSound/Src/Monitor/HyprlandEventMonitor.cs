using HyprSound.Type;
using HyprSound.Util;
using Microsoft.Extensions.Logging;

namespace HyprSound.Monitor;

public class HyprlandEventMonitor(ILogger<HyprlandEventMonitor> logger, IEventParser eventParser) : IDisposable {
    private StreamReader? _reader;
    private readonly CancellationTokenSource _cts = new();

    public event Action<IEventType>? HyprEvent;

    public async Task StartMonitor(CancellationToken externalToken = default) {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(externalToken, _cts.Token);
        var token = linkedCts.Token;

        try {
            _reader = await HyprlandIpcSocketConnector.InitIpcSocketReader();
        }
        catch (Exception ex) {
            Dispose();
            logger.LogError("监听出错: {ExMessage}", ex.Message);
        }

        while (!token.IsCancellationRequested) {
            if (_reader == null) break;
            var line = await _reader.ReadLineAsync(token);
            if (line is null) {
                break;
            }

            try {
                if (!eventParser.TryParse(line, out var hyprEvent, out var parseError) || hyprEvent is null) {
                    if (logger.IsEnabled(LogLevel.Debug))
                        logger.LogDebug("解析事件失败: {ExMessage}", parseError ?? "未知错误");
                    continue;
                }

                HyprEvent?.Invoke(hyprEvent);

                if (logger.IsEnabled(LogLevel.Debug))
                    logger.LogDebug("解析成功: '{Line}' -> '{HyprEventEventName}'", line, hyprEvent.EventName);
            }
            catch (Exception ex) {
                logger.LogWarning("处理事件失败: {ExMessage}", ex.Message);
            }
        }
    }

    public void Dispose() {
        _reader?.Dispose();
        _cts.Dispose();
        GC.SuppressFinalize(this);
    }
}