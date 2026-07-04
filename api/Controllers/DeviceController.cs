using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Pijeen.API.Models;
using Pijeen.API.Models.DTOs;
using Pijeen.API.Services;

namespace Pijeen.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DeviceController : ControllerBase
{
    private readonly IDeviceService _deviceService;
    private readonly ILogger<DeviceController> _logger;

    public DeviceController(IDeviceService deviceService, ILogger<DeviceController> logger)
    {
        _deviceService = deviceService;
        _logger = logger;
    }

    private int GetUserId()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    }

    [HttpPost("add")]
    public async Task<ActionResult<DeviceResponse>> AddDevice([FromBody] AddDeviceRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        try
        {
            var device = await _deviceService.AddDeviceAsync(
                userId,
                request.IMEI,
                request.DeviceType,
                request.DeviceName,
                request.Location
            );

            if (device == null)
            {
                return BadRequest(new { message = "Device already exists or invalid input" });
            }

            var deviceLive = await _deviceService.GetDeviceLiveStatusAsync(device.DeviceId);

            var response = new DeviceResponse
            {
                DeviceId = device.DeviceId,
                IMEI = device.IMEI,
                DeviceType = device.DeviceType,
                DeviceName = device.DeviceName,
                Location = device.Location,
                IsActive = device.IsActive,
                LiveStatus = deviceLive != null ? new DeviceLiveDto
                {
                    Status = deviceLive.Status,
                    Voltage = deviceLive.Voltage,
                    Ampere = deviceLive.Ampere,
                    RSSI = deviceLive.RSSI,
                    FaultReason = deviceLive.FaultReason,
                    LastUpdatedAt = deviceLive.LastUpdatedAt
                } : null
            };

            return Ok(new { success = true, message = "Device added successfully", device = response });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding device");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("{deviceId}")]
    public async Task<ActionResult<DeviceResponse>> GetDevice(int deviceId)
    {
        var userId = GetUserId();
        var device = await _deviceService.GetDeviceAsync(deviceId);

        if (device == null || device.UserId != userId)
        {
            return NotFound();
        }

        var deviceLive = await _deviceService.GetDeviceLiveStatusAsync(deviceId);

        var response = new DeviceResponse
        {
            DeviceId = device.DeviceId,
            IMEI = device.IMEI,
            DeviceType = device.DeviceType,
            DeviceName = device.DeviceName,
            Location = device.Location,
            IsActive = device.IsActive,
            LiveStatus = deviceLive != null ? new DeviceLiveDto
            {
                Status = deviceLive.Status,
                Voltage = deviceLive.Voltage,
                Ampere = deviceLive.Ampere,
                RSSI = deviceLive.RSSI,
                FaultReason = deviceLive.FaultReason,
                LastUpdatedAt = deviceLive.LastUpdatedAt
            } : null
        };

        return Ok(response);
    }

    [HttpGet("user/devices")]
    public async Task<ActionResult<List<DeviceResponse>>> GetUserDevices()
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        var devices = await _deviceService.GetUserDevicesAsync(userId);

        var responses = new List<DeviceResponse>();
        foreach (var device in devices)
        {
            var deviceLive = await _deviceService.GetDeviceLiveStatusAsync(device.DeviceId);
            responses.Add(new DeviceResponse
            {
                DeviceId = device.DeviceId,
                IMEI = device.IMEI,
                DeviceType = device.DeviceType,
                DeviceName = device.DeviceName,
                Location = device.Location,
                IsActive = device.IsActive,
                LiveStatus = deviceLive != null ? new DeviceLiveDto
                {
                    Status = deviceLive.Status,
                    Voltage = deviceLive.Voltage,
                    Ampere = deviceLive.Ampere,
                    RSSI = deviceLive.RSSI,
                    FaultReason = deviceLive.FaultReason,
                    LastUpdatedAt = deviceLive.LastUpdatedAt
                } : null
            });
        }

        return Ok(responses);
    }

    [HttpPost("{deviceId}/control")]
    public async Task<ActionResult> ControlDevice(int deviceId, [FromBody] DeviceControlRequest request)
    {
        var userId = GetUserId();
        var device = await _deviceService.GetDeviceAsync(deviceId);

        if (device == null || device.UserId != userId)
        {
            return NotFound();
        }

        if (!new[] { "ON", "OFF" }.Contains(request.Status.ToUpper()))
        {
            return BadRequest(new { message = "Status must be ON or OFF" });
        }

        var success = await _deviceService.ControlDeviceAsync(deviceId, request.Status, userId);

        if (!success)
        {
            return StatusCode(500, new { message = "Failed to control device" });
        }

        return Ok(new { success = true, message = "Command sent to device" });
    }

    [HttpGet("{deviceId}/audit-logs")]
    public async Task<ActionResult<List<AuditLog>>> GetAuditLogs(int deviceId, [FromQuery] int limit = 50)
    {
        var userId = GetUserId();
        var device = await _deviceService.GetDeviceAsync(deviceId);

        if (device == null || device.UserId != userId)
        {
            return NotFound();
        }

        var logs = await _deviceService.GetDeviceAuditLogsAsync(deviceId, limit);
        return Ok(logs);
    }
}
