using HyprSound.Type;

namespace HyprSound.Map;

public interface ISoundMappingResolve {
    string? GetResolvePath(IEventType eventType);
}