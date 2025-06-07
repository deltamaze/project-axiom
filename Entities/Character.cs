namespace project_axiom.Entities;

public enum CharacterClass
{
    Brawler,
    Ranger,
    Spellcaster
}

public enum ResourceTypeEnum
{
    Mana,
    Energy,
    Frenzy
}

public class Character
{
    public string Name { get; set; } = "";
    public CharacterClass Class { get; set; } = CharacterClass.Brawler;

    // Class-specific properties that will be used later
    public int MaxHealth { get; private set; }
    public int MaxResource { get; private set; }
    public ResourceTypeEnum ResourceType { get; private set; }

    // Current resource tracking (new for Section 6.8)
    public float CurrentResource { get; set; }

    public Character()
    {
        SetClassDefaults();
    }

    public Character(string name, CharacterClass characterClass)
    {
        Name = name;
        Class = characterClass;
        SetClassDefaults();
    }

    /// <summary>
    /// Update the character class and refresh defaults
    /// </summary>
    public void UpdateClass(CharacterClass newClass)
    {
        Class = newClass;
        SetClassDefaults();
    }

    private void SetClassDefaults()
    {
        switch (Class)
        {
            case CharacterClass.Brawler:
                MaxHealth = 150;
                MaxResource = 100;
                ResourceType = ResourceTypeEnum.Frenzy;
                break;
            case CharacterClass.Ranger:
                MaxHealth = 120;
                MaxResource = 120;
                ResourceType = ResourceTypeEnum.Energy;
                break;
            case CharacterClass.Spellcaster:
                MaxHealth = 100;
                MaxResource = 150;
                ResourceType = ResourceTypeEnum.Mana;
                break;
        }
        
        // Initialize current resource to maximum
        CurrentResource = MaxResource;
    }

    public string GetClassDescription()
    {
        switch (Class)
        {
            case CharacterClass.Brawler:
                return "Melee combat specialist with heavy armor and high durability.";
            case CharacterClass.Ranger:
                return "Agile fighter with ranged attacks and nature abilities.";
            case CharacterClass.Spellcaster:
                return "Master of magical arts with powerful spells and healing.";
            default:
                return "";
        }
    }

    /// <summary>
    /// Get the resource as a percentage (0.0 to 1.0) for UI display
    /// </summary>
    public float GetResourcePercentage()
    {
        return MaxResource > 0 ? CurrentResource / MaxResource : 0f;
    }

    /// <summary>
    /// Get the primary color for this character class
    /// </summary>
    public Color GetClassColor()
    {
        switch (Class)
        {
            case CharacterClass.Brawler:
                return Color.Red;
            case CharacterClass.Ranger:
                return Color.Green;
            case CharacterClass.Spellcaster:
                return Color.Blue;
            default:
                return Color.Gray;
        }
    }

    /// <summary>
    /// Get the resource bar color for this character class
    /// </summary>
    public Color GetResourceColor()
    {
        switch (Class)
        {
            case CharacterClass.Brawler:
                return Color.DarkRed;     // Rage - dark red
            case CharacterClass.Ranger:
                return Color.Gold;       // Energy - gold/yellow
            case CharacterClass.Spellcaster:
                return Color.DarkBlue;   // Mana - dark blue
            default:
                return Color.Gray;
        }
    }

    /// <summary>
    /// Consume resource for spell casting
    /// </summary>
    public bool TryConsumeResource(float amount)
    {
        if (CurrentResource >= amount)
        {
            CurrentResource = Math.Max(0, CurrentResource - amount);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Regenerate resource over time
    /// </summary>
    public void RegenerateResource(float deltaTime)
    {
        float regenRate = GetResourceRegenRate();
        CurrentResource = Math.Min(MaxResource, CurrentResource + regenRate * deltaTime);
    }

    /// <summary>
    /// Get the resource regeneration rate per second for this class
    /// </summary>
    private float GetResourceRegenRate()
    {
        switch (Class)
        {
            case CharacterClass.Brawler:
                return 5f;   // Rage regenerates slowly
            case CharacterClass.Ranger:
                return 15f;  // Energy regenerates quickly
            case CharacterClass.Spellcaster:
                return 8f;   // Mana regenerates at medium rate
            default:
                return 5f;
        }
    }
}