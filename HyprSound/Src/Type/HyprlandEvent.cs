namespace HyprSound.Type;

public interface IEventType {
}

public struct CloseWindowEventType : IEventType {
}

public struct FullscreenEventType : IEventType {
}

public struct WorkspaceChangeEventType : IEventType {
}

public struct UrgentEventType : IEventType {
}

public struct BellEventType : IEventType {
    
}

public struct ConfigReloadedEventType : IEventType {
}

public struct ChangeFloatingModeEventType : IEventType {
}