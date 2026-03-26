using HyprSound.Type;

namespace HyprSound.Util;

public static class StringExtension {
    extension(string self) {
        public IHyprlandEvent AnalysisToHyprlandEvent() {
            var index = self.IndexOf(">>", StringComparison.Ordinal);
            if (index == -1) {
                throw new InvalidDataException("未检测到 '>>',非HyprlandIpc事件");
            }

            var part = self[..index];
            return part switch {
                "closewindow" or "kill" => new CloseWindowEvent(),
                "workspacev2" => new WorkspaceChangeEvent(),
                "fullscreen" => new FullscreenEvent(),
                "urgent" => new UrgentEvent(),
                "bell" => new BellEvent(),
                "configreloaded" => new ConfigreloadedEvent(),
                "changefloatingmode" => new ChangefloatingmodeEvent(),
                _ => throw new InvalidDataException($"未解析的HyprlandIpc事件: '{part}'")
            };
        }
    }
}