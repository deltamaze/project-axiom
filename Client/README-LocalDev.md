# Local Development Server Setup

This setup allows you to run both the client and server in a single debug session for faster development.

## How It Works

When you press F5 to debug the client, it will automatically:
1. Check if a local server is running
2. If not, start an integrated server in the background
3. Connect the client to the local server
4. Allow you to set breakpoints in both client and server code

## Quick Start

1. **Enable Local Dev Mode** (default): Open `Client/DevConfig.cs` and ensure:
   ```csharp
   public static bool UseLocalDevServer => true;
   ```

2. **Debug**: Press F5 or select "Debug Client + Server (Local Dev)" from the debug menu

3. **Set Breakpoints**: You can now set breakpoints in both:
   - Client code (e.g., `ClientMovementSystem.cs`)
   - Server code (e.g., `GameSessionManager.cs`, `ServerMovementSystem.cs`)

## Configuration Options

### DevConfig.cs Settings

| Setting | Description | Default |
|---------|-------------|---------|
| `UseLocalDevServer` | Auto-start local server | `true` |
| `EnableDebugLogging` | Verbose logging | `true` |
| `LocalServerPort` | Server port | `7777` |
| `LocalServerIP` | Server IP | `127.0.0.1` |

### Switching to Remote Servers

To test against PlayFab remote servers:
1. Change `UseLocalDevServer` to `false` in `DevConfig.cs`
2. Configure your PlayFab settings in `Server/appsettings.json`
3. Press F5 to debug

## Debug Configurations

| Configuration | Purpose |
|---------------|---------|
| Debug Client + Server (Local Dev) | **Recommended**: Single-process debugging |
| Debug Server Only | Server-only debugging |
| C#: project_axiom Debug (Legacy) | Original client-only config |

## Benefits

- ✅ **Single F5 press** starts everything
- ✅ **Unified debugging** with breakpoints in both client and server
- ✅ **Shared console output** for easier debugging
- ✅ **No port conflicts** with automatic startup/shutdown
- ✅ **Easy toggle** between local and remote servers

## Troubleshooting

### "Server not responding"
- Check that port 7777 is not blocked by firewall
- Verify no other process is using port 7777

### "Connection timeout"
- The system will still work in local mode even with timeouts
- Server logs will appear in the same console as client

### Want to debug server separately?
- Use the "Debug Server Only" configuration
- Then change `UseLocalDevServer` to `false` and debug client separately
