using HyprSound.Type;

namespace HyprSound;

public static class StringExtension {
    extension(string self) {
        public IHyprlandEvent AnalysisToHyprlandEvent() {
            var index = self.IndexOf(">>", StringComparison.Ordinal);
            if (index == -1) {
                throw new InvalidDataException("未检测到 '>>',非HyprlandIpc事件");
            }

            var part = self[..index];
            return part switch {
                "openwindow" => new OpenwindowEvent(),
                _ => throw new InvalidDataException("未知的HyprlandIpc事件")
            };
        }
    }
}