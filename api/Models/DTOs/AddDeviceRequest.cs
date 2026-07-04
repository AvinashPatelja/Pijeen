namespace Pijeen.API.Models.DTOs;

public class AddDeviceRequest
{
    public string IMEI { get; set; } = null!;
    public string DeviceType { get; set; } = null!; // FC, GC, MC
    public string? DeviceName { get; set; }
    public string? Location { get; set; }
}

public class DeviceControlRequest
{
    public string Status { get; set; } = null!; // ON, OFF
}

public class DeviceResponse
{
    public int DeviceId { get; set; }
    public string IMEI { get; set; } = null!;
    public string DeviceType { get; set; } = null!;
    public string? DeviceName { get; set; }
    public string? Location { get; set; }
    public bool IsActive { get; set; }
    public DeviceLiveDto? LiveStatus { get; set; }
}

public class DeviceLiveDto
{
    public string? Status { get; set; }
    public double? Voltage { get; set; }
    public double? Ampere { get; set; }
    public int? RSSI { get; set; }
    public string? FaultReason { get; set; }
    public DateTime LastUpdatedAt { get; set; }
}
