using PlayFab;
using PlayFab.ClientModels;
using PlayFab.MultiplayerModels;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace project_axiom;

/// <summary>
/// Handles PlayFab server allocation and client connection management
/// </summary>
public class ServerAllocationManager
{
    private UdpClient _udpClient;
    private IPEndPoint _serverEndpoint;
    private bool _isConnected;
    
    public string ServerIP { get; private set; }
    public int ServerPort { get; private set; }
    public string SessionId { get; private set; }
    public bool IsConnected => _isConnected;
    public string LastError { get; private set; }

    public ServerAllocationManager()
    {
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
        // For now, always use local mode during development
        // You can change this logic later to check environment variables or config files
        return true;
    }

    /// <summary>
    /// Connect to a local development server
    /// </summary>
    private async Task<bool> ConnectToLocalServerAsync()
    {
        try
        {
            Console.WriteLine("Connecting to local development server...");
            
            ServerIP = "127.0.0.1";
            ServerPort = 7777; // Default port from our server configuration
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
                return _isConnected;
            }

            var response = await receiveTask;
            var responseMessage = System.Text.Encoding.UTF8.GetString(response.Buffer);
            
            Console.WriteLine($"Server response: {responseMessage}");
            _isConnected = true;
            
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
    }
}
