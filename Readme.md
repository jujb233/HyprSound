# HyprSound

这是一个为使用 **Hyprland** 的 Linux 系统自由添加系统音效的 dotnet 命令行工具。  
它通过监听 Hyprland 的 IPC 事件，并根据配置文件播放对应的音效音频，让你的桌面操作更加生动有趣。

---

## ✨ 特性

- 监听 Hyprland IPC 事件，实时响应
- 支持自定义音效库，灵活配置
- 简单易用的 JSON 映射配置
- 专为 NixOS 优化，支持 flake 和 direnv

---

## 📦 安装

### NixOS 用户

项目基于 NixOS 开发，推荐启用 **flakes** 和 **direnv** 进行环境管理。

1. 克隆本仓库：

   ```bash
   git clone <仓库地址>
   cd hyprsound
   ```

2. 进入开发环境并构建项目：

   ```bash
   nix develop   # 若使用 direnv 可自动加载免去这一步
   nix build .#hyprsound
   ```

3. 构建成功后，即可运行：

   ```bash
   nix run .#hyprsound
   ```

### 更新 NuGet 依赖

当你修改 `HyprSound/HyprSound.csproj` 里的 `PackageReference` 后，需要同步更新根目录的 `deps.json`：

```bash
nix build .#hyprsound.passthru.fetch-deps
./result deps.json
```

然后再执行：

```bash
nix build .#hyprsound
```

## 🚀 使用

创建音效库目录：

```bash
mkdir -p ~/.config/hyprsound/
```

将你的音效库目录（例如 `library1`）放入该目录。

### 初始化配置文件

你可以使用 `init` 命令快速生成音效映射文件的模版：

```bash
# 使用 nix 运行
nix run .#hyprsound -- init --library library1

# 或者使用 dotnet 运行
dotnet run --project HyprSound/HyprSound.csproj -- init --library library1
```

这将在 `~/.config/hyprsound/library1/` 下生成一个包含所有可用事件名称的 `sound-mapping.json` 文件。
需要您手动编写该文件建立事件与音频的映射。

### 运行程序

指定音效库名称运行程序：

```bash
# 使用 nix 运行
nix run .#hyprsound -- --library library1

# 或者在项目根目录下使用 dotnet 运行
dotnet run --project HyprSound/HyprSound.csproj -- --library library1
```

### 参数说明

- `-l, --library <library>`: **(必需)** 指定音效库名称（即音效库所在的文件夹名）。
- `-p, --path <path>`: 指定 Asset 根目录路径（默认为 `~/.config/hyprsound/`）。

程序将自动监听 Hyprland 事件，并在匹配到配置的事件时播放对应的音效。

## ⚙️ 配置
每个音效库是一个独立目录，目录名即为音效库名称。
目录中必须包含一个 `sound-mapping.json` 文件（可通过 `init` 命令生成），音效文件（目前仅支持 .wav）放置在同一目录下。

`sound-mapping.json` 示例：

```json
{
  "workspace": "audio1.wav",
  "closewindow": "audio2.wav"
}
```
> **注意**：JSON 中的键名对应 Hyprland 的事件名称。

## 📌 支持的事件

可参考项目源码中的 Src/Type/Hypr/ 目录，了解所有可用事件类型。

## ⚠️ 注意
本项目仅在 NixOS 系统上开发和测试，其他发行版未经测试，可能存在兼容性问题。

当前版本仅支持 .wav 格式的音效文件。

由于项目规模较小，部分功能可能不够完善，欢迎自行扩展或提交 PR。

## 📄 许可证
本项目为个人学习项目，开源分享，具体许可证请参见仓库中的 LICENSE 文件。
