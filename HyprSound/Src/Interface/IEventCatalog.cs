namespace HyprSound.Interface;

public interface IEventCatalog {
    string SourceName { get; }
    IReadOnlyCollection<string> EventNames { get; }
}

