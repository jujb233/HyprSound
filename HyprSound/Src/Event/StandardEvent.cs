using HyprSound.Interface;

namespace HyprSound.Event;

public enum EventKind {
    CloseWindow,
    Fullscreen,
    WorkspaceChange,
    Urgent,
    Bell,
    ConfigReloaded,
    ChangeFloatingMode,
    UsbDriveMounted,
    UsbDriveEjected,
}

public record EventCatalog : IEventCatalog {
    public IReadOnlyCollection<string> EventNames { get; } = Array.AsReadOnly(Enum.GetNames<EventKind>());
}

public record StandardEvent(string Sender, EventKind EventName) : IEventType {
}