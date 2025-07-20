using PlayFab;
using PlayFab.ClientModels;
using PlayFab.MultiplayerModels;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using project_axiom.Shared.Networking;

namespace project_axiom;

/// <summary>
/// Handles PlayFab server allocation and client connection management
/// </summary>
public class ServerAllocationManager
{
    private UdpClient _udpClient;
    private IPEndPoint _serverEndpoint;
    private bool _isConnected;
    private bool _isListening;
    private CancellationTokenSource _listeningCancellation;
    private static LocalDevServer _localDevServer;
    private readonly bool _useLocalDevServer;
    
    public string ServerIP { get; private set; }
    public int ServerPort { get; private set; }
    public string SessionId { get; private set; }
    public bool IsConnected => _isConnected;
    public string LastError { get; private set; }

    // Event for receiving server messages
    public event Action<PlayerPositionMessage> OnPlayerPositionReceived;
    public event Action<GameStateUpdateMessage> OnGameStateUpdateReceived;

    public ServerAllocationManager()
    {
        // Use the centralized development configuration
        _useLocalDevServer = DevConfig.UseLocalDevServer;
        
        // Simple initialization without logging for now
    }

    /// <summary>
    /// Request a multiplayer server from PlayFab for Training Grounds
    /// </summary>
    public async Task<bool> RequestTrainingGroundsServerAsync()
    {
        try
        {
            Console.WriteLine("Requesting Training Grounds server from PlayFab...");
            LastError = null;

            // Check if we're in local development mode
            if (IsLocalDevelopmentMode())
            {
                // Auto-start local server if enabled
                if (_useLocalDevServer)
                {
                    Console.WriteLine("[LOCAL DEV] Starting/ensuring local server is running...");
                    await StartLocalDevServerAsync();
                    Console.WriteLine("[LOCAL DEV] Waiting for server to be ready...");
                    await Task.Delay(3000); // Give server time to start
                }

                return await ConnectToLocalServerAsync();
            }

            // Request a multiplayer server from PlayFab
            var request = new RequestMultiplayerServerRequest
            {
                BuildId = "TrainingGrounds", // This should match your PlayFab build configuration
                SessionId = Guid.NewGuid().ToString(),
                PreferredRegions = new List<string> { "EastUs", "WestUs", "CentralUs" }, // Prefer US regions
                SessionCookie = "TrainingGrounds_Session"
            };

            var result = await PlayFabMultiplayerAPI.RequestMultiplayerServerAsync(request);

            if (result.Error != null)
            {
                LastError = $"PlayFab error: {result.Error.ErrorMessage}";
                Console.WriteLine($"Failed to request multiplayer server: {LastError}");
                return false;
            }

            if (result.Result?.ConnectedPlayers == null)
            {
                LastError = "No server allocation received from PlayFab";
                Console.WriteLine(LastError);
                return false;
            }

            // Extract server connection details
            var serverDetails = result.Result;
            var gamePort = serverDetails.Ports?.FirstOrDefault(p => p.Name == "gameport");
            
            if (gamePort == null)
            {
                LastError = "No game port found in server allocation";
                Console.WriteLine(LastError);
                return false;
            }

            ServerIP = serverDetails.IPV4Address;
            ServerPort = gamePort.Num;
            SessionId = request.SessionId;

            Console.WriteLine($"Server allocated: {ServerIP}:{ServerPort} (Session: {SessionId})");

            // Connect to the allocated server
            return await ConnectToServerAsync();
        }
        catch (Exception ex)
        {
            LastError = $"Exception during server allocation: {ex.Message}";
            Console.WriteLine($"Exception during server allocation: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Check if we're running in local development mode (no PlayFab cloud servers)
    /// </summary>
    private bool IsLocalDevelopmentMode()
    {
        // Check environment variable or use local mode for development
        return _useLocalDevServer;
    }

    /// <summary>
    /// Check if local server is already running
    /// </summary>
    private bool IsLocalServerRunning()
    {
        try
        {
            Console.WriteLine($"[LOCAL DEV] Checking if server is running on {DevConfig.LocalServerIP}:{DevConfig.LocalServerPort}...");
            
            using var client = new UdpClient();
            client.Client.ReceiveTimeout = 1000; // 1 second timeout
            
            var endpoint = new IPEndPoint(IPAddress.Parse(DevConfig.LocalServerIP), DevConfig.LocalServerPort);
            
            // Try to send a ping message
            var testData = System.Text.Encoding.UTF8.GetBytes("PING");
            client.Send(testData, testData.Length, endpoint);
            
            Console.WriteLine("[LOCAL DEV] Ping sent, server appears reachable");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LOCAL DEV] Server not detected: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Start the local development server
    /// </summary>
    private async Task StartLocalDevServerAsync()
    {
        try
        {
            Console.WriteLine("[LOCAL DEV] Initializing local server instance...");
            
            if (_localDevServer == null)
            {
                _localDevServer = new LocalDevServer();
            }
            
            if (!_localDevServer.IsRunning)
            {
                Console.WriteLine("[LOCAL DEV] Starting server...");
                await _localDevServer.StartAsync();
                Console.WriteLine("[LOCAL DEV] Server startup completed");
            }
            else
            {
                Console.WriteLine("[LOCAL DEV] Server already running");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LOCAL DEV] Error starting local server: {ex.Message}");
            Console.WriteLine($"[LOCAL DEV] Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    /// <summary>
    /// Connect to a local development server
    /// </summary>
    private async Task<bool> ConnectToLocalServerAsync()
    {
        try
        {
            Console.WriteLine("Connecting to local development server...");
            
            ServerIP = DevConfig.LocalServerIP;
            ServerPort = DevConfig.LocalServerPort;
            SessionId = $"local_session_{Guid.NewGuid():N}";

            return await ConnectToServerAsync();
        }
        catch (Exception ex)
        {
            LastError = $"Failed to connect to local server: {ex.Message}";
            Console.WriteLine($"Failed to connect to local server: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Establish UDP connection to the game server
    /// </summary>
    private async Task<bool> ConnectToServerAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(ServerIP))
            {
                LastError = "No server IP available";
                return false;
            }

            Console.WriteLine($"Connecting to game server at {ServerIP}:{ServerPort}...");

            // Create UDP client for game communication
            _udpClient = new UdpClient();
            _serverEndpoint = new IPEndPoint(IPAddress.Parse(ServerIP), ServerPort);

            // Send a simple connection test message
            var testMessage = System.Text.Encoding.UTF8.GetBytes($"CONNECT:{SessionId}");
            await _udpClient.SendAsync(testMessage, testMessage.Length, _serverEndpoint);

            // Wait briefly for a response (simple connection validation)
            var receiveTask = _udpClient.ReceiveAsync();
            var timeoutTask = Task.Delay(5000); // 5 second timeout

            var completedTask = await Task.WhenAny(receiveTask, timeoutTask);
            
            if (completedTask == timeoutTask)
            {
                LastError = "Connection timeout - server may not be running";
                Console.WriteLine($"Connection timeout to server {ServerIP}:{ServerPort}");
                // For local development, we'll still consider this a success
                _isConnected = IsLocalDevelopmentMode();
                
                if (_isConnected)
                {
                    // Start listening for server messages even on timeout in local mode
                    _ = StartListeningForMessagesAsync();
                }
                
                return _isConnected;
            }

            var response = await receiveTask;
            var responseMessage = System.Text.Encoding.UTF8.GetString(response.Buffer);
            
            Console.WriteLine($"Server response: {responseMessage}");
            _isConnected = true;
            
            // Start listening for server messages
            _ = StartListeningForMessagesAsync();

            return true;
        }
        catch (Exception ex)
        {
            LastError = $"Failed to connect to server: {ex.Message}";
            Console.WriteLine($"Failed to connect to server: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Start listening for messages from the server
    /// </summary>
    private async Task StartListeningForMessagesAsync()
    {
        try
        {
            _isListening = true;
            _listeningCancellation = new CancellationTokenSource();

            while (_isListening)
            {
                var receiveResult = await _udpClient.ReceiveAsync();
                
                // Process the received message
                ProcessServerMessage(receiveResult.Buffer);
            }
        }
        catch (OperationCanceledException)
        {
            // Listening was cancelled, simply exit the method
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while listening for server messages: {ex.Message}");
        }
    }

    /// <summary>
    /// Process a message received from the server
    /// </summary>
    private void ProcessServerMessage(byte[] message)
    {
        try
        {
            var json = System.Text.Encoding.UTF8.GetString(message);
            
            // Check if it's a JSON message
            if (json.StartsWith("{"))
            {
                using var document = System.Text.Json.JsonDocument.Parse(json);
                var messageType = document.RootElement.GetProperty("MessageType").GetString();

                switch (messageType)
                {
                    case "PlayerPosition":
                        var positionMessage = NetworkMessage.Deserialize<PlayerPositionMessage>(message);
                        if (positionMessage != null)
                            OnPlayerPositionReceived?.Invoke(positionMessage);
                        break;
                        
                    case "GameStateUpdate":
                        var gameStateMessage = NetworkMessage.Deserialize<GameStateUpdateMessage>(message);
                        if (gameStateMessage != null)
                            OnGameStateUpdateReceived?.Invoke(gameStateMessage);
                        break;
                        
                    default:
                        Console.WriteLine($"Unknown message type: {messageType}");
                        break;
                }
            }
            else
            {
                // Handle text messages (heartbeat, etc.)
                Console.WriteLine($"Received text message: {json}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing server message: {ex.Message}");
        }
    }

    /// <summary>
    /// Send a message to the connected game server
    /// </summary>
    public async Task<bool> SendMessageAsync(byte[] message)
    {
        if (!_isConnected || _udpClient == null || _serverEndpoint == null)
        {
            return false;
        }

        try
        {
            await _udpClient.SendAsync(message, message.Length, _serverEndpoint);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send message to server: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Disconnect from the game server and clean up resources
    /// </summary>
    public void Disconnect()
    {
        Console.WriteLine("Disconnecting from game server...");
        
        _isConnected = false;
        _isListening = false;
        _listeningCancellation?.Cancel();
        _udpClient?.Close();
        _udpClient?.Dispose();
        _udpClient = null;
        _serverEndpoint = null;
        
        ServerIP = null;
        ServerPort = 0;
        SessionId = null;
    }

    /// <summary>
    /// Dispose of resources
    /// </summary>
    public void Dispose()
    {
        Disconnect();
        
        // Clean up local dev server
        if (_localDevServer != null)
        {
            _localDevServer.StopAsync().Wait(5000); // Wait up to 5 seconds for shutdown
        }
    }
}
