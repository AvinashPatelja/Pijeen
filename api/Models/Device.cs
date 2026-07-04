namespace Pijeen.API.Models;

public class Device
{
    public int DeviceId { get; set; }
    public string IMEI { get; set; } = null!;
    public string DeviceType { get; set; } = null!; // FC, GC, MC
    public int UserId { get; set; }
    public string? DeviceName { get; set; }
    public string? Location { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
