using HyprSound.Interface;
using Microsoft.Extensions.Logging;
using SDL3;

namespace HyprSound.Player;

public class SdlPlayer : IPlayer {
    private readonly ISoundMappingResolve _soundMappingResolve;
    private readonly ILogger<SdlPlayer> _logger;
    private IntPtr _currentStream = IntPtr.Zero;
    private bool _disposed;

    public SdlPlayer(ISoundMappingResolve soundMappingResolve, ILogger<SdlPlayer> logger) {
        _soundMappingResolve = soundMappingResolve;
        _logger = logger;
        if (!SDL.Init(SDL.InitFlags.Audio)) {
            throw new InvalidOperationException($"SDL 初始化失败: {SDL.GetError()}");
        }
    }

    public void Play(IEventType eventType) {
        var path = _soundMappingResolve.GetResolvePath(eventType);

        if (path is null or "" || !File.Exists(path)) {
            _logger.LogError("音频文件: {Path} 不存在", path);
            return;
        }

        if (_currentStream != IntPtr.Zero) {
            SDL.DestroyAudioStream(_currentStream);
            _currentStream = IntPtr.Zero;
        }

        if (!SDL.LoadWAV(path, out var spec, out var wavData, out var wavLength)) {
            _logger.LogError("加载 WAV 失败: {GetError}", SDL.GetError());
            return;
        }

        var stream = SDL.OpenAudioDeviceStream(
            SDL.AudioDeviceDefaultPlayback,
            in spec,
            null,
            IntPtr.Zero
        );
        SDL.PutAudioStreamData(stream, wavData, (int)wavLength);
        SDL.FreeWAV(wavData);
        if (!SDL.ResumeAudioStreamDevice(stream)) {
            _logger.LogError("无法恢复音频流: {GetError}", SDL.GetError());
            SDL.DestroyAudioStream(stream);
            return;
        }

        _currentStream = stream;
        
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug("正在播放: {Path}", path);
    }

    public void Dispose() {
        if (_disposed) return;
        SDL.DestroyAudioStream(_currentStream);
        _currentStream = IntPtr.Zero;

        SDL.Quit();
        _disposed = true;

        GC.SuppressFinalize(this);
    }
}