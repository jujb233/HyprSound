namespace HyprSound.Type;

public interface IHyprlandEvent {
}

public struct CloseWindowEvent : IHyprlandEvent {
}

public struct FullscreenEvent : IHyprlandEvent {
}

public struct WorkspaceChangeEvent : IHyprlandEvent {
}

public struct UrgentEvent : IHyprlandEvent {
}

public struct BellEvent : IHyprlandEvent {
    
}

public struct ConfigreloadedEvent : IHyprlandEvent {
}

public struct ChangefloatingmodeEvent : IHyprlandEvent {
}