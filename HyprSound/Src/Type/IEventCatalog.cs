namespace HyprSound.Type;

public interface IEventCatalog {
    string SourceName { get; }
    IReadOnlyCollection<string> EventNames { get; }
}

