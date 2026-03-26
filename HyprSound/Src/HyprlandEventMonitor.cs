using HyprSound.Util;

namespace HyprSound;

public class HyprlandEventMonitor : IDisposable {
    private StreamReader? _reader;
    private readonly CancellationTokenSource _cts = new();

    public async Task StartMonitor(CancellationToken externalToken = default) {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(externalToken, _cts.Token);
        var token = linkedCts.Token;
        
        try {
            _reader = await HyprlandIpcSocketConnector.InitIpcSocketReader();

            while (!token.IsCancellationRequested) {
                var line = await _reader.ReadLineAsync(token);
                if (line is null) {
                    break;
                }

                try {
                    var evt = line.AnalysisToHyprlandEvent();
                    Console.WriteLine(evt.ToString());
                }
                catch (Exception ex) {
                    Console.WriteLine($"解析事件失败: {ex.Message}");
                }
            }
        }
        catch (OperationCanceledException) {
        }
        catch (Exception ex) {
            Console.WriteLine($"监听出错: {ex.Message}");
        }
        finally {
            Dispose();
        }
    }

    public void Dispose() {
        _reader?.Dispose();
        _cts.Dispose();
        GC.SuppressFinalize(this);
    }
}