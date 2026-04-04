using HyprSound.Interface;

namespace HyprSound.Usb.Event;

public struct UsbEvents : IEventCatalog {
    public UsbEvents() {
    }

    public const string DriveMounted = "UsbDriveMounted";
    public const string DriveEjected = "UsbDriveEjected";

    public string SourceName => "UsbDevice";

    public IReadOnlyCollection<string> EventNames { get; } = [
        DriveMounted,
        DriveEjected
    ];
}

public struct UsbDriveMountedEventType : IEventType {
    public UsbDriveMountedEventType() {
    }

    public string EventName { get; } = UsbEvents.DriveMounted;
}

public struct UsbDriveEjectedEventType : IEventType {
    public UsbDriveEjectedEventType() {
    }

    public string EventName { get; } = UsbEvents.DriveEjected;
}