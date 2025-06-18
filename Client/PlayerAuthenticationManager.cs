using PlayFab.ClientModels;
using PlayFab;
using project_axiom.Shared;
using System.Text.Json;

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
    }    /// <summary>
    /// Save a character to PlayFab Player Data
    /// </summary>
    /// <param name="character">The character to save</param>
    /// <param name="onSuccess">Callback on successful save</param>
    /// <param name="onError">Callback on error</param>
    public static async void SaveCharacter(Character character, Action<string> onSuccess = null, Action<string> onError = null)
    {
        if (!IsAuthenticated)
        {
            onError?.Invoke("Player not authenticated");
            return;
        }

        try
        {
            // Serialize character data to JSON
            var characterData = new
            {
                Name = character.Name,
                Class = character.Class.ToString(),
                MaxHealth = character.MaxHealth,
                MaxResource = character.MaxResource,
                ResourceType = character.ResourceType.ToString()
            };

            string characterJson = JsonSerializer.Serialize(characterData);

            var request = new UpdateUserDataRequest
            {
                Data = new Dictionary<string, string>
                {
                    { "character", characterJson }
                }
            };

            var result = await PlayFabClientAPI.UpdateUserDataAsync(request);
            if (result.Error == null)
            {
                onSuccess?.Invoke($"Character '{character.Name}' saved successfully!");
            }
            else
            {
                onError?.Invoke($"Failed to save character: {result.Error.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            onError?.Invoke($"Error serializing character data: {ex.Message}");
        }
    }    /// <summary>
    /// Load character data from PlayFab Player Data
    /// </summary>
    /// <param name="onSuccess">Callback with loaded character on success</param>
    /// <param name="onError">Callback on error</param>
    public static async void LoadCharacter(Action<Character> onSuccess = null, Action<string> onError = null)
    {
        if (!IsAuthenticated)
        {
            onError?.Invoke("Player not authenticated");
            return;
        }

        var request = new GetUserDataRequest
        {
            Keys = new List<string> { "character" }
        };

        try
        {
            var result = await PlayFabClientAPI.GetUserDataAsync(request);
            if (result.Error == null)
            {                if (result.Result.Data != null && result.Result.Data.ContainsKey("character"))
                {
                    string characterJson = result.Result.Data["character"].Value;
                    
                    // Deserialize character data using JsonDocument for proper handling
                    using (var document = JsonDocument.Parse(characterJson))
                    {
                        var root = document.RootElement;
                        var character = new Character();
                        
                        if (root.TryGetProperty("Name", out var nameElement))
                            character.Name = nameElement.GetString() ?? "";
                        
                        if (root.TryGetProperty("Class", out var classElement) && 
                            Enum.TryParse<CharacterClass>(classElement.GetString(), out var characterClass))
                            character.UpdateClass(characterClass);

                        onSuccess?.Invoke(character);
                    }
                }
                else
                {
                    // No character data found
                    onSuccess?.Invoke(null);
                }
            }
            else
            {
                onError?.Invoke($"Failed to load character: {result.Error.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            onError?.Invoke($"Error deserializing character data: {ex.Message}");
        }
    }

    /// <summary>
    /// Check if player has a saved character
    /// </summary>
    /// <param name="onResult">Callback with boolean result</param>
    /// <param name="onError">Callback on error</param>
    public static void HasSavedCharacter(Action<bool> onResult = null, Action<string> onError = null)
    {
        LoadCharacter(
            character => onResult?.Invoke(character != null),
            error => onError?.Invoke(error)
        );
    }

    /// <summary>
    /// Delete the player's character data from PlayFab
    /// </summary>
    /// <param name="onSuccess">Callback on successful deletion</param>
    /// <param name="onError">Callback on error</param>
    public static async void DeleteCharacter(Action<string> onSuccess = null, Action<string> onError = null)
    {
        if (!IsAuthenticated)
        {
            onError?.Invoke("Player not authenticated");
            return;
        }

        try
        {
            var request = new UpdateUserDataRequest
            {
                KeysToRemove = new List<string> { "character" }
            };

            var result = await PlayFabClientAPI.UpdateUserDataAsync(request);
            if (result.Error == null)
            {
                onSuccess?.Invoke("Character deleted successfully!");
            }
            else
            {
                onError?.Invoke($"Failed to delete character: {result.Error.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            onError?.Invoke($"Error deleting character data: {ex.Message}");
        }
    }
}
