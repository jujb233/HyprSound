namespace HyprSound.Type.Hypr;

public sealed class HyprlandEventCatalog : IEventCatalog {
    public string SourceName => "Hyprland";

    public IReadOnlyCollection<string> EventNames => HyprlandEvents.All;
}

