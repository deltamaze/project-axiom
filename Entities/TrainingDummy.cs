namespace project_axiom.Entities;

/// <summary>
/// Represents a static training dummy in the training grounds
/// </summary>
public class TrainingDummy
{
    public Vector3 Position { get; set; }
    public float MaxHealth { get; private set; }
    public float CurrentHealth { get; set; }
    public bool IsAlive => CurrentHealth > 0;
    public string Name { get; set; }

    // Visual properties
    public Color PrimaryColor { get; set; }
    public Color SecondaryColor { get; set; }
    public float Scale { get; set; } = 1.2f; // Slightly larger than player cubes

    public TrainingDummy(Vector3 position, string name = "Training Dummy")
    {
        Position = position;
        Name = name;
        MaxHealth = 100f;
        CurrentHealth = MaxHealth;
        
        // Default training dummy colors (orange/brown theme)
        PrimaryColor = new Color(200, 100, 50);   // Orange-brown
        SecondaryColor = new Color(150, 75, 25);  // Darker brown
    }

    /// <summary>
    /// Take damage and return true if the dummy was destroyed
    /// </summary>
    public bool TakeDamage(float damage)
    {
        if (!IsAlive) return false;

        CurrentHealth = Math.Max(0, CurrentHealth - damage);
        return !IsAlive;
    }

    /// <summary>
    /// Reset the dummy to full health
    /// </summary>
    public void Reset()
    {
        CurrentHealth = MaxHealth;
    }

    /// <summary>
    /// Get health as a percentage (0.0 to 1.0)
    /// </summary>
    public float GetHealthPercentage()
    {
        return MaxHealth > 0 ? CurrentHealth / MaxHealth : 0f;
    }
}