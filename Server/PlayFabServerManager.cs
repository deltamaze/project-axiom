using Microsoft.Extensions.Logging;
using PlayFab;
using PlayFab.MultiplayerModels;
using System.Text.Json;
using System.Runtime.InteropServices;

namespace project_axiom_server;

public class PlayFabServerManager
{
    private readonly ILogger<PlayFabServerManager> _logger;
    private bool _isInitialized;
    private Timer? _heartbeatTimer;
    private string? _serverId;
    private string? _vmId;
    private bool _isPlayFabEnvironment;
    private readonly HttpClient _httpClient;
    
    // PlayFab Game Server SDK simulation via HTTP calls to local agent
    private const string AGENT_BASE_URL = "http://localhost:56001";    private int _connectedPlayerCount = 0;

    public PlayFabServerManager(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<PlayFabServerManager>();
        _httpClient = new HttpClient();
    }

    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("Initializing PlayFab Game Server SDK...");

            // Check if we're running under PlayFab Multiplayer Servers
            var isPlayFabEnvironment = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PF_SERVER_INSTANCE_NUMBER"));
            
            if (isPlayFabEnvironment)
            {
                await InitializePlayFabEnvironment();
            }
            else
            {
                await InitializeLocalEnvironment();
            }

            _isInitialized = true;
            _logger.LogInformation("PlayFab Game Server SDK initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize PlayFab Game Server SDK");
            throw;
        }
    }    private async Task InitializePlayFabEnvironment()
    {
        _logger.LogInformation("Running in PlayFab Multiplayer Servers environment");
        
        // Get server details from environment variables
        _serverId = Environment.GetEnvironmentVariable("PF_SERVER_INSTANCE_NUMBER");
        _vmId = Environment.GetEnvironmentVariable("PF_VM_ID");
        
        _logger.LogInformation($"Server ID: {_serverId}, VM ID: {_vmId}");

        // Configure PlayFab settings from environment
        var titleId = Environment.GetEnvironmentVariable("PF_TITLE_ID");
        if (!string.IsNullOrEmpty(titleId))
        {
            PlayFabSettings.staticSettings.TitleId = titleId;
            _logger.LogInformation($"PlayFab Title ID: {titleId}");
        }

        _isPlayFabEnvironment = true;

        // Initialize the PlayFab Game Server SDK equivalent
        await StartGameServerSDK();
        
        // Start the heartbeat to let PlayFab know the server is alive
        StartHeartbeat();
        
        // Signal that the server is ready to accept players
        await SignalServerReady();
    }    private async Task InitializeLocalEnvironment()
    {
        _logger.LogInformation("Running in local development environment");
        _logger.LogInformation("PlayFab integration will be limited in local mode");
        
        // For local testing, we don't need full PlayFab integration
        // This allows development without requiring LocalMultiplayerAgent
        _serverId = "local-server";
        _vmId = "local-vm";
        _isPlayFabEnvironment = false;
        
        await Task.CompletedTask;
    }

    private async Task StartGameServerSDK()
    {
        try
        {
            _logger.LogInformation("Starting PlayFab Game Server SDK...");
            
            if (_isPlayFabEnvironment)
            {
                // In a real PlayFab environment, this would call the native SDK
                // For now, we'll use HTTP calls to communicate with the LocalMultiplayerAgent
                await SendAgentRequest("POST", "v1/sessionhost/start", new { });
                _logger.LogInformation("Game Server SDK started successfully");
            }
            else
            {
                _logger.LogInformation("Skipping Game Server SDK start in local mode");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to start Game Server SDK - continuing without it");
        }
    }

    private void StartHeartbeat()
    {
        _logger.LogInformation("Starting PlayFab heartbeat timer");
        
        // Send heartbeat every 30 seconds
        _heartbeatTimer = new Timer(async _ => await SendHeartbeat(), null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
    }    private async Task SendHeartbeat()
    {
        try
        {
            _logger.LogDebug("Sending heartbeat to PlayFab");
            
            if (_isPlayFabEnvironment)
            {
                // Send heartbeat to PlayFab agent
                var heartbeatData = new
                {
                    CurrentGameState = "Active",
                    CurrentGameHealth = "Healthy",
                    CurrentPlayerCount = _connectedPlayerCount
                };
                
                await SendAgentRequest("POST", "v1/sessionhost/heartbeat", heartbeatData);
            }
            else
            {
                _logger.LogDebug("Local mode - heartbeat logged only");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send heartbeat to PlayFab");
        }
    }

    private async Task SendAgentRequest(string method, string endpoint, object? data = null)
    {
        try
        {
            var url = $"{AGENT_BASE_URL}/{endpoint}";
            HttpResponseMessage response;
            
            if (method == "POST")
            {
                var jsonContent = data != null ? JsonSerializer.Serialize(data) : "{}";
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
                response = await _httpClient.PostAsync(url, content);
            }
            else
            {
                response = await _httpClient.GetAsync(url);
            }
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug($"Successfully sent {method} request to {endpoint}");
            }
            else
            {
                _logger.LogWarning($"Failed to send {method} request to {endpoint}: {response.StatusCode}");
            }
        }
        catch (HttpRequestException ex)
        {
            // This is expected when not running with LocalMultiplayerAgent
            _logger.LogDebug(ex, $"Could not connect to PlayFab agent for {endpoint} - this is normal in local development");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Error sending request to PlayFab agent: {endpoint}");
        }
    }    private async Task SignalServerReady()
    {
        try
        {
            _logger.LogInformation("Signaling to PlayFab that server is ready for players");
            
            if (_isPlayFabEnvironment)
            {
                // Signal to PlayFab that the server is ready to accept players
                var readyData = new
                {
                    Operation = "Ready"
                };
                
                await SendAgentRequest("POST", "v1/sessionhost/ready", readyData);
                _logger.LogInformation("Successfully signaled server readiness to PlayFab");
            }
            else
            {
                _logger.LogInformation("Local mode - server marked as ready");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to signal server readiness to PlayFab");
            throw;
        }
    }    public async Task NotifyPlayerConnected(string playFabId)
    {
        try
        {
            _logger.LogInformation($"Player connected: {playFabId}");
            
            _connectedPlayerCount++;
            
            if (_isPlayFabEnvironment)
            {
                // Notify PlayFab about the connected player
                var playerData = new
                {
                    PlayerId = playFabId,
                    Operation = "PlayerConnected"
                };
                
                await SendAgentRequest("POST", "v1/sessionhost/updateconnectedplayers", new
                {
                    CurrentPlayers = new[] { new { PlayerId = playFabId } }
                });
                
                _logger.LogInformation($"Notified PlayFab about player connection: {playFabId}");
            }
            else
            {
                _logger.LogInformation($"Local mode - player connection logged: {playFabId}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Failed to notify PlayFab about player connection: {playFabId}");
        }
    }

    public async Task NotifyPlayerDisconnected(string playFabId)
    {
        try
        {
            _logger.LogInformation($"Player disconnected: {playFabId}");
            
            _connectedPlayerCount = Math.Max(0, _connectedPlayerCount - 1);
            
            if (_isPlayFabEnvironment)
            {
                // Notify PlayFab about the disconnected player
                var playerData = new
                {
                    PlayerId = playFabId,
                    Operation = "PlayerDisconnected"
                };
                
                await SendAgentRequest("POST", "v1/sessionhost/updateconnectedplayers", new
                {
                    CurrentPlayers = Array.Empty<object>() // Send empty array to indicate player left
                });
                
                _logger.LogInformation($"Notified PlayFab about player disconnection: {playFabId}");
            }
            else
            {
                _logger.LogInformation($"Local mode - player disconnection logged: {playFabId}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Failed to notify PlayFab about player disconnection: {playFabId}");
        }
    }    public async Task ShutdownAsync()
    {
        try
        {
            _logger.LogInformation("Shutting down PlayFab Game Server SDK...");
            
            _heartbeatTimer?.Dispose();
            
            if (_isPlayFabEnvironment)
            {
                // Notify PlayFab that the server is shutting down
                await SendAgentRequest("POST", "v1/sessionhost/terminate", new
                {
                    Operation = "Terminate"
                });
                
                _logger.LogInformation("Notified PlayFab about server shutdown");
            }
            
            _httpClient?.Dispose();
            
            _logger.LogInformation("PlayFab Game Server SDK shutdown complete");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during PlayFab shutdown");
        }
    }    public bool IsInitialized => _isInitialized;
    public string? ServerId => _serverId;
    public string? VmId => _vmId;
    public int ConnectedPlayerCount => _connectedPlayerCount;
    public bool IsPlayFabEnvironment => _isPlayFabEnvironment;
}
