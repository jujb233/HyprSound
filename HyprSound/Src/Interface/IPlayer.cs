using HyprSound.Event;

namespace HyprSound.Interface;

public interface IPlayer : IDisposable {
    void Play(IEventType eventType);
}