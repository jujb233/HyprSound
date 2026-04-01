using HyprSound.Type;
using HyprSound.Type.Hypr;

namespace HyprSound.Resolve;

public sealed class HyprlandEventResolve : IEventParser {
    public bool TryParse(string input, out IEventType? eventType, out string? error) {
        eventType = null;
        error = null;

        var index = input.IndexOf(">>", StringComparison.Ordinal);
        if (index == -1) {
            error = "未检测到 '>>'，非 Hyprland IPC 事件";
            return false;
        }

        var part = input[..index];
        eventType = part switch {
            "closewindow" or "kill" => new CloseWindowEventType(),
            "workspacev2" => new WorkspaceChangeEventType(),
            "fullscreen" => new FullscreenEventType(),
            "urgent" => new UrgentEventType(),
            "bell" => new BellEventType(),
            "configreloaded" => new ConfigReloadedEventType(),
            "changefloatingmode" => new ChangeFloatingModeEventType(),
            _ => null
        };

        if (eventType is null) {
            error = $"未映射的 Hyprland IPC 事件: '{part}'";
            return false;
        }

        return true;
    }
}

