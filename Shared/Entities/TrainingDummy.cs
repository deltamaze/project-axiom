using Microsoft.Xna.Framework;

namespace project_axiom.Shared.Entities;

/// <summary>
/// Represents a static training dummy - shared between client and server
/// </summary>
public class TrainingDummy
{
    public int Id { get; set; }
    public Vector3 Position { get; set; }
    public float MaxHealth { get; private set; }
    public float CurrentHealth { get; set; }
    public bool IsAlive => CurrentHealth > 0;
    public string Name { get; set; }

    // Visual properties (client-side usage)
    public Color PrimaryColor { get; set; }
    public Color SecondaryColor { get; set; }
    public float Scale { get; set; } = 1.2f; // Slightly larger than player cubes

    // Death state properties
    public bool IsDead => CurrentHealth <= 0;
    public DateTime? DeathTime { get; private set; }

    public TrainingDummy() : this(0, Vector3.Zero, "Training Dummy")
    {
        // Parameterless constructor for serialization
    }

    public TrainingDummy(int id, Vector3 position, string name = "Training Dummy")
    {
        Id = id;
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
        
        // Mark death time when dummy dies
        if (IsDead && !DeathTime.HasValue)
        {
            DeathTime = DateTime.Now;
        }
        
        return !IsAlive;
    }

    /// <summary>
    /// Reset the dummy to full health
    /// </summary>
    public void Reset()
    {
        CurrentHealth = MaxHealth;
        DeathTime = null;
    }

    /// <summary>
    /// Get health as a percentage (0.0 to 1.0)
    /// </summary>
    public float GetHealthPercentage()
    {
        return MaxHealth > 0 ? CurrentHealth / MaxHealth : 0f;
    }

    /// <summary>
    /// Check if the dummy is within targeting range of a position
    /// </summary>
    public bool IsInRange(Vector3 sourcePosition, float maxRange)
    {
        float distance = Vector3.Distance(sourcePosition, Position);
        return distance <= maxRange;
    }
}
