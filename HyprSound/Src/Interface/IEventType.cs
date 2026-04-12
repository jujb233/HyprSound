using HyprSound.Event;

namespace HyprSound.Interface;

public interface IEventType {
    public string Sender { get; init; }
    public EventKind EventName { get; init; }
}