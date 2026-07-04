namespace Pijeen.API.Models;

public class DeviceLive
{
    public int DeviceLiveId { get; set; }
    public int DeviceId { get; set; }
    public string IMEI { get; set; } = null!;
    public string DeviceType { get; set; } = null!;
    public string? Status { get; set; } // ON, OFF
    public double? Voltage { get; set; }
    public double? Ampere { get; set; }
    public int? RSSI { get; set; }
    public string? FaultReason { get; set; }
    public int Valve { get; set; } = 0;
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
}
