using System.Text.Json;
using HyprSound.Type;

namespace HyprSound.Map;

public class JsonMappingResolve : ISoundMappingResolve {
    private readonly Dictionary<string, string?> _mapping;
    private readonly string _pathToAsset;

    public JsonMappingResolve(string pathToJsonFile, string pathToAsset) {
        _pathToAsset = pathToAsset;
        var jsonFile = File.ReadAllText(pathToJsonFile);
        _mapping = JsonSerializer.Deserialize<Dictionary<string, string?>>(jsonFile) ?? [];
    }

    public string? GetResolvePath(IEventType eventType) {
        var key = eventType.ToString();
        if (key is null) {
            throw new NullReferenceException("要解析的事件为空,无法做映射处理");
        }

        if (!_mapping.TryGetValue(key, out var pathToAudio))
            throw new KeyNotFoundException($"未找到事件 {key} 的音频映射");

        return pathToAudio is null
            ? null
            : Path.Combine(_pathToAsset, pathToAudio);
    }
}