namespace HyprSound.Interface;

public interface IEventResolve {
    bool TryParse(string input, out IEventType? eventType, out string? error);
}

