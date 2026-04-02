using System.Text.Json;
using System.Text.Json.Serialization;
using HyprSound.Type;
using Microsoft.Extensions.Logging;

namespace HyprSound.Map;

public class JsonMappingResolve : ISoundMappingResolve {
    private readonly ILogger<JsonMappingResolve> _logger;
    private readonly Dictionary<string, string?> _mapping;
    private readonly string _pathToLibrary;
    private readonly HashSet<string> _knownEvents;

    public JsonMappingResolve(
        string pathToAsset,
        string libraryName,
        IEnumerable<IEventCatalog> catalogs,
        ILogger<JsonMappingResolve> logger
    ) {
        _logger = logger;
        _pathToLibrary = Path.Combine(pathToAsset, libraryName);
        var catalogsArray = catalogs as IEventCatalog[] ?? catalogs.ToArray();

        _knownEvents = catalogsArray
            .SelectMany(static catalog => catalog.EventNames)
            .Where(static name => name is not ("" or " "))
            .ToHashSet(StringComparer.Ordinal);

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

        if (_logger.IsEnabled(LogLevel.Information)) {
            _logger.LogInformation("找到映射文件: {path}", pathToJsonFile);
        }

        ValidateMappings(catalogsArray);
    }

    public string? GetResolvePath(IEventType eventType) {
        var key = eventType.EventName;
        if (key is null) {
            throw new NullReferenceException("要解析的事件为空，无法做映射处理");
        }

        if (!_mapping.TryGetValue(key, out var pathToAudio)) {
            _logger.LogWarning("事件“{Key}”未在映射文件中配置，触发时将不会播放音频。", key);
            return null;
        }

        if (!string.IsNullOrWhiteSpace(pathToAudio))
            return Path.Combine(_pathToLibrary, pathToAudio);

        _logger.LogWarning("事件“{Key}”的映射值为空，将返回 null,若触发该事件不会播放音频.", key);

        return null;
    }

    private void ValidateMappings(IEnumerable<IEventCatalog> catalogs) {
        var mappedKeys = _mapping.Keys.ToHashSet(StringComparer.Ordinal);
        var missingKey = _knownEvents.Where(name => !mappedKeys.Contains(name)).Order(StringComparer.Ordinal)
            .ToArray();
        var unknownInJson = mappedKeys.Where(name => !_knownEvents.Contains(name)).Order(StringComparer.Ordinal)
            .ToArray();
        var nullOrBlankMapping = _mapping
            .Where(static pair => string.IsNullOrWhiteSpace(pair.Value))
            .Select(static pair => pair.Key)
            .Order(StringComparer.Ordinal)
            .ToArray();

        foreach (var catalog in catalogs) {
            _logger.LogInformation("已注册事件目录: {SourceName} ({Count})", catalog.SourceName,
                catalog.EventNames.Count);
        }

        if (missingKey.Length > 0) {
            _logger.LogWarning("已知但未映射的事件({Count}): {Names}", missingKey.Length, string.Join(", ", missingKey));
        }

        if (unknownInJson.Length > 0) {
            _logger.LogWarning("映射文件中的未知事件({Count}): {Names}", unknownInJson.Length, string.Join(", ", unknownInJson));
        }

        if (nullOrBlankMapping.Length > 0) {
            _logger.LogWarning("映射值为空的事件({Count}): {Names}", nullOrBlankMapping.Length,
                string.Join(", ", nullOrBlankMapping));
        }

        if (missingKey.Length == 0 && unknownInJson.Length == 0 && nullOrBlankMapping.Length == 0) {
            _logger.LogInformation("事件映射检查通过，共 {Count} 个映射项。", _mapping.Count);
        }
    }
}

[JsonSerializable(typeof(Dictionary<string, string?>))]
public partial class SoundMappingJsonContext : JsonSerializerContext {
}