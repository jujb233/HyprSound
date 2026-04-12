namespace HyprSound.Interface;

public interface IEventCatalog {
    IReadOnlyCollection<string> EventNames { get; }
}

