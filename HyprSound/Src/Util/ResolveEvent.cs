using HyprSound.Hypr;
using HyprSound.Type;

namespace HyprSound.Util;

public static class ResolveEvent {
    extension(string self) {
        public IEventType ResolveHyprlandEvent() {
            var index = self.IndexOf(">>", StringComparison.Ordinal);
            if (index == -1) {
                throw new InvalidDataException("未检测到 '>>',非HyprlandIpc事件");
            }

            var part = self[..index];
            return part switch {
                "closewindow" or "kill" => new CloseWindowEventType(),
                "workspacev2" => new WorkspaceChangeEventType(),
                "fullscreen" => new FullscreenEventType(),
                "urgent" => new UrgentEventType(),
                "bell" => new BellEventType(),
                "configreloaded" => new ConfigReloadedEventType(),
                "changefloatingmode" => new ChangeFloatingModeEventType(),
                _ => throw new InvalidDataException($"未映射的HyprlandIpc事件: '{part}'")
            };
        }
    }
}