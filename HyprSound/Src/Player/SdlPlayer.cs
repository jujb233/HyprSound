using HyprSound.Type;
using SDL3;

namespace HyprSound.Player;

public class SdlPlayer : IPlayer{
    public void Play(IEventType eventType) {
        Console.WriteLine("play " + eventType);
    }
}