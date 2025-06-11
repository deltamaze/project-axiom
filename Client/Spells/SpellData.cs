namespace project_axiom.Spells;

/// <summary>
/// Defines the data structure for a spell/ability
/// </summary>
public class SpellData
{
    public string Name { get; set; }
    public string Description { get; set; }
    public float ResourceCost { get; set; }
    public float CooldownDuration { get; set; }
    public float Range { get; set; }
    public float Damage { get; set; }
    public SpellTargetType TargetType { get; set; }
    public ResourceTypeEnum RequiredResourceType { get; set; }

    public SpellData(string name, string description, float resourceCost, float cooldownDuration, 
                     float range, float damage, SpellTargetType targetType, ResourceTypeEnum requiredResourceType)
    {
        Name = name;
        Description = description;
        ResourceCost = resourceCost;
        CooldownDuration = cooldownDuration;
        Range = range;
        Damage = damage;
        TargetType = targetType;
        RequiredResourceType = requiredResourceType;
    }
}

/// <summary>
/// Defines the types of spell targeting
/// </summary>
public enum SpellTargetType
{
    SingleTarget,
    SelfTarget,
    AreaOfEffect,
    Projectile
}
