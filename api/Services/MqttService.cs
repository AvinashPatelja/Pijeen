using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Server;
using System.Text.Json;
using Pijeen.API.Data;
using Pijeen.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Pijeen.API.Services;

public interface IMqttService
{
    Task ConnectAsync();
    Task DisconnectAsync();
    Task PublishAsync(string topic, object payload);
    Task SubscribeAsync(string topic);
}

public class MqttService : IMqttService, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<MqttService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private IMqttClient? _mqttClient;
    private readonly List<string> _subscribedTopics = new();

    public MqttService(IConfiguration configuration, ILogger<MqttService> logger, IServiceProvider serviceProvider)
    {
        _configuration = configuration;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task ConnectAsync()
    {
        try
        {
            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();

            var broker = _configuration["Mqtt:Broker"] ?? "broker.emqx.io";
            var port = int.Parse(_configuration["Mqtt:Port"] ?? "1883");
            var clientId = _configuration["Mqtt:ClientId"] ?? "PijeenAPI";

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(broker, port)
                .WithClientId(clientId)
                .WithCleanSession()
                .Build();

            _mqttClient.DisconnectedAsync += HandleDisconnected;
            _mqttClient.ApplicationMessageReceivedAsync += HandleMessageReceived;

            await _mqttClient.ConnectAsync(options, CancellationToken.None);
            _logger.LogInformation("Connected to MQTT broker: {Broker}:{Port}", broker, port);

            // Resubscribe to previous topics if any
            foreach (var topic in _subscribedTopics)
            {
                await SubscribeAsync(topic);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to MQTT broker");
            throw;
        }
    }

    public async Task DisconnectAsync()
    {
        if (_mqttClient?.IsConnected == true)
        {
            await _mqttClient.DisconnectAsync();
            _logger.LogInformation("Disconnected from MQTT broker");
        }
    }

    public async Task PublishAsync(string topic, object payload)
    {
        if (_mqttClient?.IsConnected != true)
        {
            _logger.LogWarning("MQTT client not connected, attempting to reconnect...");
            await ConnectAsync();
        }

        try
        {
            var json = JsonSerializer.Serialize(payload);
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(json)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            await _mqttClient!.PublishAsync(message);
            _logger.LogInformation("Published to topic: {Topic}", topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing to topic: {Topic}", topic);
            throw;
        }
    }

    public async Task SubscribeAsync(string topic)
    {
        if (_mqttClient?.IsConnected != true)
        {
            _logger.LogWarning("MQTT client not connected, cannot subscribe");
            return;
        }

        try
        {
            if (!_subscribedTopics.Contains(topic))
            {
                _subscribedTopics.Add(topic);
            }

            var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
                .WithTopicFilter(new MqttTopicFilterBuilder().WithTopic(topic).Build())
                .Build();

            await _mqttClient.SubscribeAsync(subscribeOptions);
            _logger.LogInformation("Subscribed to topic: {Topic}", topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to topic: {Topic}", topic);
        }
    }

    private async Task HandleMessageReceived(MqttApplicationMessageReceivedEventArgs args)
    {
        try
        {
            var topic = args.ApplicationMessage.Topic;
            var payload = System.Text.Encoding.UTF8.GetString(args.ApplicationMessage.PayloadSegment);

            _logger.LogInformation("Message received on topic: {Topic}, Payload: {Payload}", topic, payload);

            // Parse device status response: devices/{IMEI}/fc/status
            if (topic.StartsWith("devices/") && topic.Contains("/fc/status"))
            {
                await HandleFieldControllerStatus(payload);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling MQTT message");
        }
    }

    private async Task HandleFieldControllerStatus(string payload)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var data = JsonSerializer.Deserialize<JsonElement>(payload);

            if (data.TryGetProperty("imei", out var imeiElement) && imeiElement.ValueKind == JsonValueKind.String)
            {
                var imei = imeiElement.GetString();

                var deviceLive = await context.DeviceLive.FirstOrDefaultAsync(d => d.IMEI == imei);
                if (deviceLive != null)
                {
                    // Update DeviceLive
                    if (data.TryGetProperty("status", out var statusElement))
                        deviceLive.Status = statusElement.GetString();
                    if (data.TryGetProperty("Voltage", out var voltageElement))
                        deviceLive.Voltage = voltageElement.TryGetDouble(out var voltage) ? voltage : null;
                    if (data.TryGetProperty("Ampere", out var ampereElement))
                        deviceLive.Ampere = ampereElement.TryGetDouble(out var ampere) ? ampere : null;
                    if (data.TryGetProperty("rssi", out var rssiElement))
                        deviceLive.RSSI = rssiElement.TryGetInt32(out var rssi) ? rssi : null;
                    if (data.TryGetProperty("faultReason", out var faultElement))
                        deviceLive.FaultReason = faultElement.GetString();

                    deviceLive.LastUpdatedAt = DateTime.UtcNow;

                    // Log to AuditLog
                    var auditLog = new AuditLog
                    {
                        DeviceId = deviceLive.DeviceId,
                        IMEI = deviceLive.IMEI,
                        DeviceType = deviceLive.DeviceType,
                        Status = deviceLive.Status,
                        Voltage = deviceLive.Voltage,
                        Ampere = deviceLive.Ampere,
                        RSSI = deviceLive.RSSI,
                        FaultReason = deviceLive.FaultReason,
                        Valve = deviceLive.Valve,
                        ActionType = "HardwareUpdate",
                        CreatedAt = DateTime.UtcNow
                    };

                    context.DeviceLive.Update(deviceLive);
                    context.AuditLogs.Add(auditLog);
                    await context.SaveChangesAsync();

                    _logger.LogInformation("Updated DeviceLive for IMEI: {IMEI}", imei);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling field controller status update");
        }
    }

    private Task HandleDisconnected(MqttClientDisconnectedEventArgs args)
    {
        _logger.LogWarning("Disconnected from MQTT broker. Attempting to reconnect...");
        // In production, implement exponential backoff reconnection
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _mqttClient?.Dispose();
    }
}
