using System.Net.Sockets;

namespace HyprSound.Util;

public static class HyprlandIpcSocketConnector{
    private static string InstanceSignature {
        get {
            field = Environment.GetEnvironmentVariable("HYPRLAND_INSTANCE_SIGNATURE") ??
                    throw new NullReferenceException("环境变量 'HYPRLAND_INSTANCE_SIGNATURE' 为空");
            return field;
        }
    }

    private static string RuntimeDir {
        get {
            field = Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR") ??
                    throw new NullReferenceException("环境变量 'XDG_RUNTIME_DIR' 为空");
            return field;
        }
    }

    public static async Task<StreamReader> InitIpcSocketReader() {
        var socketPath = Path.Combine(RuntimeDir, "hypr", InstanceSignature, ".socket2.sock");

        var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
        var endpoint = new UnixDomainSocketEndPoint(socketPath);
        await socket.ConnectAsync(endpoint);

        var stream = new NetworkStream(socket);
        var reader = new StreamReader(stream);

        return reader;
    }
}