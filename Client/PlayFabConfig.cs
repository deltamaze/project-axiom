using PlayFab;

namespace project_axiom;

/// <summary>
/// Configuration for PlayFab integration
/// </summary>
public static class PlayFabConfig
{
    /// <summary>
    /// Your PlayFab Title ID - this is public information that can be in source code
    /// Get this from your PlayFab Game Manager dashboard
    /// </summary>
    public const string TitleId = "YOUR_PLAYFAB_TITLE_ID_HERE"; // TODO: Replace with your actual Title ID
    
    /// <summary>
    /// Initialize PlayFab settings
    /// </summary>
    public static void Initialize()
    {
        PlayFabSettings.staticSettings.TitleId = TitleId;
        
        // Optional: Set development environment URL if using a development environment
        // PlayFabSettings.staticSettings.DeveloperSecretKey = "YOUR_DEV_SECRET"; // Never use in production
        
        // Enable verbose logging for development (disable in production)
#if DEBUG
        // Request type setting removed as it's not available in this version
#endif
    }
}
