namespace project_axiom.Shared.Spells;

/// <summary>
/// Result of attempting to cast a spell
/// </summary>
public class CastResult
{
    public bool Success { get; set; }
    public string? FailureReason { get; set; }
    public SpellData? SpellCast { get; set; }
    public float DamageDealt { get; set; }
}
