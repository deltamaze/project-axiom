namespace project_axiom.Spells;

/// <summary>
/// Manages spell cooldowns
/// </summary>
public class SpellCooldownManager
{
    private Dictionary<string, float> _cooldowns = new Dictionary<string, float>();

    /// <summary>
    /// Update all cooldowns by reducing them by the elapsed time
    /// </summary>
    public void Update(float deltaTime)
    {
        var keysToRemove = new List<string>();
        
        foreach (var kvp in _cooldowns.ToList())
        {
            var newCooldown = kvp.Value - deltaTime;
            if (newCooldown <= 0)
            {
                keysToRemove.Add(kvp.Key);
            }
            else
            {
                _cooldowns[kvp.Key] = newCooldown;
            }
        }

        foreach (var key in keysToRemove)
        {
            _cooldowns.Remove(key);
        }
    }

    /// <summary>
    /// Start a cooldown for a spell
    /// </summary>
    public void StartCooldown(string spellName, float duration)
    {
        _cooldowns[spellName] = duration;
    }

    /// <summary>
    /// Check if a spell is on cooldown
    /// </summary>
    public bool IsOnCooldown(string spellName)
    {
        return _cooldowns.ContainsKey(spellName);
    }

    /// <summary>
    /// Get remaining cooldown time for a spell
    /// </summary>
    public float GetRemainingCooldown(string spellName)
    {
        return _cooldowns.TryGetValue(spellName, out float remaining) ? remaining : 0f;
    }

    /// <summary>
    /// Get cooldown progress as a percentage (0.0 = ready, 1.0 = just started cooldown)
    /// </summary>
    public float GetCooldownProgress(string spellName, float totalCooldown)
    {
        if (!IsOnCooldown(spellName)) return 0f;
        
        float remaining = GetRemainingCooldown(spellName);
        return remaining / totalCooldown;
    }
}
