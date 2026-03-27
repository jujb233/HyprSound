namespace HyprSound.Type;

public interface ISoundMappingResolve {
    string? GetResolvePath(IEventType eventType);
}