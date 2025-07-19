# Project Axiom - Dedicated Server

This is the dedicated server component for Project Axiom, designed to run on Azure PlayFab Multiplayer Servers.

## Features

- **PlayFab Game Server SDK Integration**: Communicates with PlayFab for server lifecycle management
- **UDP Game Server**: Handles client connections and game traffic
- **Player Session Management**: Tracks connected players and their game state
- **Heartbeat System**: Maintains connection with PlayFab and clients
- **Local Development Support**: Can run locally without PlayFab for testing

## Development Setup

### Prerequisites
- .NET 8.0 SDK
- Visual Studio Code
- PlayFab account and Title ID (for cloud deployment)

### Local Testing

1. **Build the server**:
   ```bash
   dotnet build
   ```

2. **Run locally**:
   ```bash
   dotnet run
   ```
   
   Or use the batch file:
   ```bash
   start-server.bat
   ```

### PlayFab Local Testing (Step 19)

1. **Download LocalMultiplayerAgent** from Microsoft
2. **Configure the agent** to point to your built server executable
3. **Run the agent** to simulate PlayFab environment locally

### PlayFab Cloud Deployment (Step 20)

1. **Build and package**:
   ```bash
   dotnet publish -c Release -o ./publish
   ```

2. **Create zip file** containing the published files

3. **Upload to PlayFab** via the Game Manager dashboard

## Configuration

### Environment Variables (Set by PlayFab)
- `PF_TITLE_ID`: PlayFab Title ID
- `PF_SERVER_INSTANCE_NUMBER`: Unique server instance identifier  
- `PF_VM_ID`: Virtual machine identifier
- `GamePort`: Port for game traffic (default: 7777)

### Local Development Variables
Set these in your environment or batch file for local testing:
- `PF_TITLE_ID`: Your PlayFab Title ID (optional for local testing)
- `GamePort`: Port to listen on (default: 7777)

## Architecture

### Core Components

- **GameServer**: Main server orchestrator
- **PlayFabServerManager**: Handles PlayFab SDK integration and communication
- **GameSessionManager**: Manages player sessions and game state
- **ConnectedPlayer**: Represents a connected player and their state

### Message Protocol

Basic UDP message protocol:
- `CONNECT:PlayFabId` - Player connection request
- `DISCONNECT:PlayFabId` - Player disconnect notification  
- `HEARTBEAT` - Keep-alive message
- `CONNECT_ACK:SUCCESS` - Connection acknowledgment
- `HEARTBEAT_ACK` - Heartbeat response

## Next Steps (Ready for Step 19)

1. **Download and configure LocalMultiplayerAgent**
2. **Test server startup and PlayFab communication**
3. **Implement client-server networking protocol**
4. **Add authoritative movement and spell casting**
5. **Integrate with shared game entities and logic**

## File Structure

```
Server/
├── Program.cs                  # Entry point
├── GameServer.cs              # Main server class
├── PlayFabServerManager.cs    # PlayFab SDK integration
├── GameSessionManager.cs      # Player session management
├── appsettings.json          # Configuration
├── start-server.bat          # Local testing script
└── project-axiom-server.csproj # Project file
```
