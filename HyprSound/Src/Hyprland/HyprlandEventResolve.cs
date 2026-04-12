using HyprSound.Event;
using HyprSound.Interface;

namespace HyprSound.Hyprland;

public sealed class HyprlandEventResolve : IEventResolve {
    public bool TryParse(string input, out IEventType? eventType, out string? error) {
        eventType = null;

        var index = input.IndexOf(">>", StringComparison.Ordinal);
        if (index == -1) {
            error = "未检测到 '>>'，非 Hyprland IPC 事件";
            return false;
        }

        var part = input[..index];
        var template = new StandardEvent("Hyprland", default);
        eventType = part switch {
            "closewindow" or "kill" => template with { EventName = EventKind.CloseWindow },
            "workspacev2" => template with { EventName = EventKind.WorkspaceChange },
            "fullscreen" => template with { EventName = EventKind.Fullscreen },
            "urgent" => template with { EventName = EventKind.Urgent },
            "bell" => template with { EventName = EventKind.Bell },
            "configreloaded" => template with { EventName = EventKind.ConfigReloaded },
            "changefloatingmode" => template with { EventName = EventKind.ChangeFloatingMode },
            _ => null
        };

        if (eventType is null) {
            error = $"未映射的 Hyprland IPC 事件: '{part}'";
            return false;
        }

        error = null;
        return true;
    }
}