using HyprSound.Interface;

namespace HyprSound.Hyprland.Event;

public static class HyprlandEvents {
    public const string CloseWindow = "CloseWindow";
    public const string Fullscreen = "Fullscreen";
    public const string WorkspaceChange = "WorkspaceChange";
    public const string Urgent = "Urgent";
    public const string Bell = "Bell";
    public const string ConfigReloaded = "ConfigReloaded";
    public const string ChangeFloatingMode = "ChangeFloatingMode";

    public static readonly string[] All = [
        CloseWindow,
        Fullscreen,
        WorkspaceChange,
        Urgent,
        Bell,
        ConfigReloaded,
        ChangeFloatingMode
    ];
}

public struct CloseWindowEventType : IEventType {
    public CloseWindowEventType() {
    }

    public string EventName { get; } = HyprlandEvents.CloseWindow;
}

public struct FullscreenEventType : IEventType {
    public FullscreenEventType() {
    }

    public string EventName { get; } = HyprlandEvents.Fullscreen;
}

public struct WorkspaceChangeEventType : IEventType {
    public WorkspaceChangeEventType() {
    }

    public string EventName { get; } = HyprlandEvents.WorkspaceChange;
}

public struct UrgentEventType : IEventType {
    public UrgentEventType() {
    }

    public string EventName { get; } = HyprlandEvents.Urgent;
}

public struct BellEventType : IEventType {
    public BellEventType() {
    }

    public string EventName { get; } = HyprlandEvents.Bell;
}

public struct ConfigReloadedEventType : IEventType {
    public ConfigReloadedEventType() {
    }

    public string EventName { get; } = HyprlandEvents.ConfigReloaded;
}

public struct ChangeFloatingModeEventType : IEventType {
    public ChangeFloatingModeEventType() {
    }

    public string EventName { get; } = HyprlandEvents.ChangeFloatingMode;
}