using Microsoft.Extensions.Logging;
using project_axiom_server;
using System;
using System.Threading.Tasks;

namespace project_axiom
{
    public class LocalDevServer
    {
        private GameServer _gameServer;
        private readonly ILoggerFactory _loggerFactory;
        private bool _isRunning;

        public LocalDevServer()
        {
            // Create logger factory for the integrated server
            _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddConsole()
                    .AddDebug()
                    .SetMinimumLevel(LogLevel.Information);
            });
        }

        public async Task StartAsync()
        {
            if (_isRunning) 
            {
                Console.WriteLine("[LOCAL DEV] Server already running, skipping startup");
                return;
            }

            try
            {
                Console.WriteLine("[LOCAL DEV] Creating GameServer instance...");
                _gameServer = new GameServer(_loggerFactory);
                
                Console.WriteLine("[LOCAL DEV] Starting GameServer...");
                await _gameServer.StartAsync();
                
                _isRunning = true;
                Console.WriteLine("[LOCAL DEV] ✅ Integrated server started successfully on port 7777");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LOCAL DEV] ❌ Failed to start server: {ex.Message}");
                Console.WriteLine($"[LOCAL DEV] Stack trace: {ex.StackTrace}");
                _isRunning = false;
                _gameServer = null;
                throw;
            }
        }

        public async Task StopAsync()
        {
            if (!_isRunning || _gameServer == null) return;

            Console.WriteLine("[LOCAL DEV] Stopping integrated game server...");
            await _gameServer.StopAsync();
            _gameServer = null;
            _isRunning = false;
        }

        public bool IsRunning => _isRunning;
    }
}
