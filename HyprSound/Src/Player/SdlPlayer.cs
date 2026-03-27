using HyprSound.Type;
using SDL3;

namespace HyprSound.Player;

public class SdlPlayer : IPlayer, IDisposable {
    private readonly ISoundMappingResolve _soundMappingResolve;
    private IntPtr _currentStream = IntPtr.Zero;
    private bool _disposed;

    public SdlPlayer(ISoundMappingResolve soundMappingResolve) {
        _soundMappingResolve = soundMappingResolve;
        if (!SDL.Init(SDL.InitFlags.Audio)) {
            throw new InvalidOperationException($"SDL 初始化失败: {SDL.GetError()}");
        }
    }

    public void Play(IEventType eventType) {
        var path = _soundMappingResolve.GetResolvePath(eventType);

        if (path is null or "" || !File.Exists(path)) {
            Console.WriteLine($"{path} 不存在");
            return;
        }

        if (_currentStream != IntPtr.Zero) {
            SDL.DestroyAudioStream(_currentStream);
            _currentStream = IntPtr.Zero;
        }

        if (!SDL.LoadWAV(path, out var spec, out var wavData, out var wavLength)) {
            Console.WriteLine($"加载 WAV 失败: {SDL.GetError()}");
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
            Console.WriteLine($"无法恢复音频流: {SDL.GetError()}");
            SDL.DestroyAudioStream(stream);
            return;
        }

        _currentStream = stream;
        Console.WriteLine($"正在播放: {path}");
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