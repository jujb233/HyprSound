using HyprSound.Event;

namespace HyprSound.Interface;

public interface ISoundMappingResolve {
    string? GetResolvePath(IEventType eventType);
}