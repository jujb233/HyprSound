using HyprSound.Interface;

namespace HyprSound.Hyprland.Event;

public sealed class HyprlandEventCatalog : IEventCatalog {
    public string SourceName => "Hyprland";

    public IReadOnlyCollection<string> EventNames => HyprlandEvents.All;
}

