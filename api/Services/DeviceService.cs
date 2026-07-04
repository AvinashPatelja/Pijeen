using Pijeen.API.Data;
using Pijeen.API.Models;
using Pijeen.API.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Pijeen.API.Services;

public interface IDeviceService
{
    Task<Device?> AddDeviceAsync(int userId, string imei, string deviceType, string? deviceName, string? location);
    Task<Device?> GetDeviceAsync(int deviceId);
    Task<List<Device>> GetUserDevicesAsync(int userId);
    Task<DeviceLive?> GetDeviceLiveStatusAsync(int deviceId);
    Task<bool> ControlDeviceAsync(int deviceId, string status, int? userId);
    Task<List<AuditLog>> GetDeviceAuditLogsAsync(int deviceId, int limit = 50);
}

public class DeviceService : IDeviceService
{
    private readonly ApplicationDbContext _context;
    private readonly IMqttService _mqttService;
    private readonly ILogger<DeviceService> _logger;

    public DeviceService(ApplicationDbContext context, IMqttService mqttService, ILogger<DeviceService> logger)
    {
        _context = context;
        _mqttService = mqttService;
        _logger = logger;
    }

    public async Task<Device?> AddDeviceAsync(int userId, string imei, string deviceType, string? deviceName, string? location)
    {
        // Check if device already exists
        var existingDevice = await _context.Devices.FirstOrDefaultAsync(d => d.IMEI == imei && d.UserId == userId);
        if (existingDevice != null)
        {
            _logger.LogWarning("Device with IMEI {IMEI} already exists for user {UserId}", imei, userId);
            return null;
        }

        var device = new Device
        {
            IMEI = imei,
            DeviceType = deviceType,
            UserId = userId,
            DeviceName = deviceName ?? $"{deviceType}-{imei}",
            Location = location,
            IsActive = true
        };

        _context.Devices.Add(device);
        await _context.SaveChangesAsync();

        // Initialize DeviceLive record
        var deviceLive = new DeviceLive
        {
            DeviceId = device.DeviceId,
            IMEI = imei,
            DeviceType = deviceType,
            Status = "OFF",
            Valve = deviceType == "FC" ? 0 : 0,
            LastUpdatedAt = DateTime.UtcNow
        };

        _context.DeviceLive.Add(deviceLive);

        // Log initial entry
        var auditLog = new AuditLog
        {
            DeviceId = device.DeviceId,
            IMEI = imei,
            DeviceType = deviceType,
            Status = "OFF",
            Valve = deviceType == "FC" ? 0 : 0,
            ActionType = "System",
            ActionBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Device added: {IMEI} ({DeviceType}) for user {UserId}", imei, deviceType, userId);

        return device;
    }

    public async Task<Device?> GetDeviceAsync(int deviceId)
    {
        return await _context.Devices.FirstOrDefaultAsync(d => d.DeviceId == deviceId);
    }

    public async Task<List<Device>> GetUserDevicesAsync(int userId)
    {
        return await _context.Devices
            .Where(d => d.UserId == userId && d.IsActive)
            .ToListAsync();
    }

    public async Task<DeviceLive?> GetDeviceLiveStatusAsync(int deviceId)
    {
        var device = await GetDeviceAsync(deviceId);
        if (device == null) return null;

        return await _context.DeviceLive.FirstOrDefaultAsync(dl => dl.DeviceId == deviceId);
    }

    public async Task<bool> ControlDeviceAsync(int deviceId, string status, int? userId)
    {
        var device = await GetDeviceAsync(deviceId);
        if (device == null) return false;

        var deviceLive = await GetDeviceLiveStatusAsync(deviceId);
        if (deviceLive == null) return false;

        try
        {
            // Publish MQTT command
            var payload = new
            {
                deviceType = device.DeviceType,
                imei = device.IMEI,
                Valve = device.DeviceType == "FC" ? 0 : 0,
                status = status.ToUpper()
            };

            var topic = $"device/{device.IMEI}/{device.DeviceType.ToLower()}/status";
            await _mqttService.PublishAsync(topic, payload);

            // Log the action
            var auditLog = new AuditLog
            {
                DeviceId = deviceId,
                IMEI = device.IMEI,
                DeviceType = device.DeviceType,
                Status = status.ToUpper(),
                Valve = device.DeviceType == "FC" ? 0 : 0,
                ActionType = "UserCommand",
                ActionBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Device control command sent: {IMEI} -> {Status}", device.IMEI, status);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error controlling device {DeviceId}", deviceId);
            return false;
        }
    }

    public async Task<List<AuditLog>> GetDeviceAuditLogsAsync(int deviceId, int limit = 50)
    {
        return await _context.AuditLogs
            .Where(al => al.DeviceId == deviceId)
            .OrderByDescending(al => al.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }
}
