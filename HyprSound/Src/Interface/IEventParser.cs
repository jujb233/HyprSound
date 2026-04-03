namespace HyprSound.Interface;

public interface IEventParser {
    bool TryParse(string input, out IEventType? eventType, out string? error);
}

