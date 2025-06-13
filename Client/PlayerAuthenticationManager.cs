using PlayFab.ClientModels;

namespace project_axiom;

/// <summary>
/// Manages the authenticated player's information and state
/// </summary>
public static class PlayerAuthenticationManager
{
    /// <summary>
    /// The currently authenticated player's PlayFab ID
    /// </summary>
    public static string PlayFabId { get; private set; } = string.Empty;
    
    /// <summary>
    /// Whether the player is currently authenticated
    /// </summary>
    public static bool IsAuthenticated => !string.IsNullOrEmpty(PlayFabId);
    
    /// <summary>
    /// Set the authenticated player information
    /// </summary>
    /// <param name="playFabId">The PlayFab ID from login/register result</param>
    public static void SetAuthenticatedPlayer(string playFabId)
    {
        PlayFabId = playFabId ?? string.Empty;
    }
    
    /// <summary>
    /// Clear the authentication state (for logout)
    /// </summary>
    public static void ClearAuthentication()
    {
        PlayFabId = string.Empty;
    }
}
