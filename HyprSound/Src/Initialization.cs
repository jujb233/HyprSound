using System.Text.Json;
using HyprSound.MappingResolve;
using Microsoft.Extensions.Logging;


namespace HyprSound;

public class Initialization(ILogger<Initialization> logger) {
    public void InitJsonFile(string pathToLibrary, IEnumerable<string> eventNames) {
        var jsonFile = Path.Combine(pathToLibrary, "sound-mapping.json");

        if (File.Exists(jsonFile)) {
            Console.Error.WriteLine($"warn: 配置文件已存在：{jsonFile}"); // TODO 防止log信息比交互提示信息晚出
            Console.Error.Flush();
            Console.Error.Write("是否将原文件重命名为 .bak 并创建新配置文件？(y/N): ");
            var response = Console.ReadLine()?.Trim().ToLower();
            if (response != "y" && response != "yes") {
                logger.LogInformation("用户取消操作，未修改配置文件。");
                return;
            }

            var bakFile = jsonFile + ".bak";
            var bakIndex = 1;
            while (File.Exists(bakFile)) {
                bakFile = $"{jsonFile}.bak{bakIndex++}";
            }

            try {
                File.Move(jsonFile, bakFile);
                if (logger.IsEnabled(LogLevel.Information))
                    logger.LogInformation("原文件已备份为：{BakFilePath}", bakFile);
            }
            catch (Exception ex) {
                logger.LogError(ex, "备份原文件失败，操作中止。");
                return;
            }
        }

        Directory.CreateDirectory(pathToLibrary);

        var initMap = eventNames.ToDictionary(
            name => name, string? (_) => null
        );

        var tempFile = jsonFile + ".tmp";
        try {
            var json = JsonSerializer.Serialize(initMap, SoundMappingJsonContext.Default.DictionaryStringString);
            File.WriteAllText(tempFile, json);

            if (logger.IsEnabled(LogLevel.Debug))
                logger.LogDebug("中间文件写入成功: {TempFile}", tempFile);

            File.Move(tempFile, jsonFile, overwrite: true);

            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("已生成配置文件模板：{JsonFilePath}", jsonFile);
        }
        catch (Exception ex) {
            logger.LogError(ex, "生成配置文件失败，正在回滚...");

            if (File.Exists(tempFile)) {
                try {
                    File.Delete(tempFile);
                }
                catch (Exception cleanupEx) {
                    logger.LogWarning(cleanupEx, "删除临时文件失败：{TempFile}", tempFile);

                    var bakFilePath = jsonFile + ".bak";
                    if (File.Exists(bakFilePath)) {
                        try {
                            if (File.Exists(jsonFile))
                                File.Delete(jsonFile);
                            File.Move(bakFilePath, jsonFile);

                            if (logger.IsEnabled(LogLevel.Information))
                                logger.LogInformation("已从备份恢复原文件：{JsonFilePath}", jsonFile);
                        }
                        catch (Exception restoreEx) {
                            logger.LogError(restoreEx, "恢复备份失败，请手动处理备份文件：{BakFilePath}", bakFilePath);
                        }
                    }
                }
            }
        }
    }
}