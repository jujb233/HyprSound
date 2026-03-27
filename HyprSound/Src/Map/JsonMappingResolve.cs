using System.Text.Json;
using System.Text.Json.Serialization;
using HyprSound.Type;
using Microsoft.Extensions.Logging;

namespace HyprSound.Map;

public class JsonMappingResolve : ISoundMappingResolve {
    private readonly ILogger<JsonMappingResolve> _logger;
    private readonly Dictionary<string, string?> _mapping;
    private readonly string _pathToLibrary;

    public JsonMappingResolve(string pathToAsset, string libraryName, ILogger<JsonMappingResolve> logger) {
        _logger = logger;
        _pathToLibrary = Path.Combine(pathToAsset, libraryName);

        var pathToJsonFile = Path.Combine(pathToAsset, libraryName, "sound-mapping.json");
        if (!File.Exists(pathToJsonFile))
            throw new FileNotFoundException($"未找到映射文件：{pathToJsonFile}");

        var jsonFile = File.ReadAllText(pathToJsonFile);
        if (jsonFile is null or "" or " ") {
            throw new InvalidOperationException($"映射文件 {pathToJsonFile} 为空，请检查文件内容。");
        }

        try {
            _mapping = JsonSerializer.Deserialize(jsonFile, SoundMappingJsonContext.Default.DictionaryStringString)
                       ?? [];
        }
        catch (JsonException ex) {
            throw new InvalidOperationException($"解析映射文件 {pathToJsonFile} 时出错：JSON 格式无效。详细信息：{ex.Message}", ex);
        }
    }

    public string? GetResolvePath(IEventType eventType) {
        var key = eventType.EventName;
        if (key is null) {
            throw new NullReferenceException("要解析的事件为空，无法做映射处理");
        }

        if (!_mapping.TryGetValue(key, out var pathToAudio)) {
            throw new KeyNotFoundException($"映射文件中缺少事件“{key}”的映射项。");
        }

        if (pathToAudio is not null)
            return Path.Combine(_pathToLibrary, pathToAudio);

        _logger.LogWarning("事件“{Key}”的映射值为空，将返回 null,若触发该事件不会播放音频.", key);

        return null;
    }
}

[JsonSerializable(typeof(Dictionary<string, string?>))]
public partial class SoundMappingJsonContext : JsonSerializerContext {
}