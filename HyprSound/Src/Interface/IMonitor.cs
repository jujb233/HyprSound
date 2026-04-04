namespace HyprSound.Interface;

public interface IMonitor : IDisposable {
    public Task StartMonitor(CancellationToken externalToken = default);
}