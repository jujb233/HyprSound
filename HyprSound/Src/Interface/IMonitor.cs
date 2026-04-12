namespace HyprSound.Interface;

public interface IMonitor : IDisposable {
    event Action<IEventType> EventOccurred;
    Task StartAsync(CancellationToken cancellationToken);
}