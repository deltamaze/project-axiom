namespace project_axiom
{
    /// <summary>
    /// Development configuration settings
    /// Toggle these flags to control local vs remote server behavior
    /// </summary>
    public static class DevConfig
    {
        /// <summary>
        /// Set to true to automatically start a local server when debugging
        /// Set to false to connect to remote PlayFab servers
        /// </summary>
        public static bool UseLocalDevServer => true;

        /// <summary>
        /// Set to true to enable verbose debug logging
        /// </summary>
        public static bool EnableDebugLogging => true;

        /// <summary>
        /// Local server port (should match server configuration)
        /// </summary>
        public static int LocalServerPort => 7777;

        /// <summary>
        /// Local server IP address
        /// </summary>
        public static string LocalServerIP => "127.0.0.1";
    }
}
