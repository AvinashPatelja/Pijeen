namespace Pijeen.API.Models;

public class AuditLog
{
    public int AuditLogId { get; set; }
    public int DeviceId { get; set; }
    public string IMEI { get; set; } = null!;
    public string DeviceType { get; set; } = null!;
    public string? Status { get; set; }
    public double? Voltage { get; set; }
    public double? Ampere { get; set; }
    public int? RSSI { get; set; }
    public string? FaultReason { get; set; }
    public int Valve { get; set; }
    public string? ActionType { get; set; } // UserCommand, HardwareUpdate, System
    public int? ActionBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
