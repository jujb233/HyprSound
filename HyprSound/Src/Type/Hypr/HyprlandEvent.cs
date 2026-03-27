using HyprSound.Type;

namespace HyprSound.Hypr;

public struct CloseWindowEventType : IEventType {
    public CloseWindowEventType() {
    }

    public string EventName { get; } = "CloseWindow";
}

public struct FullscreenEventType : IEventType {
    public FullscreenEventType() {
    }

    public string EventName { get; } = "Fullscreen";
}

public struct WorkspaceChangeEventType : IEventType {
    public WorkspaceChangeEventType() {
    }

    public string EventName { get; } = "WorkspaceChange";
}

public struct UrgentEventType : IEventType {
    public UrgentEventType() {
    }

    public string EventName { get; } = "Urgent";
}

public struct BellEventType : IEventType {
    public BellEventType() {
    }

    public string EventName { get; } = "Bell";
}

public struct ConfigReloadedEventType : IEventType {
    public ConfigReloadedEventType() {
    }

    public string EventName { get; } = "ConfigReloaded";
}

public struct ChangeFloatingModeEventType : IEventType {
    public ChangeFloatingModeEventType() {
    }

    public string EventName { get; set; } = "ChangeFloatingMode";
}