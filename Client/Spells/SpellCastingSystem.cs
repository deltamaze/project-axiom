using project_axiom.Shared.Spells;

namespace project_axiom.Spells;

/// <summary>
/// Manages spell casting and ability usage
/// </summary>
public class SpellCastingSystem
{
    private Dictionary<int, SpellData> _equippedSpells = new Dictionary<int, SpellData>();
    private SpellCooldownManager _cooldownManager = new SpellCooldownManager();

    public SpellCastingSystem()
    {
        InitializeDefaultSpells();
    }

    /// <summary>
    /// Initialize default spells for each class
    /// </summary>
    private void InitializeDefaultSpells()
    {
        // For now, we'll just add the Brawler Slam spell to slot 1 (index 0)
        var slamSpell = new SpellData(
            name: "Slam",
            description: "A powerful melee attack that slams the target.",
            resourceCost: 20f,
            cooldownDuration: 1.0f,
            range: 3.0f, // Melee range
            damage: 25f,
            targetType: SpellTargetType.SingleTarget,
            requiredResourceType: ResourceTypeEnum.Frenzy
        );

        _equippedSpells[0] = slamSpell; // Slot 1 (index 0)
    }

    /// <summary>
    /// Update the spell system (cooldowns, etc.)
    /// </summary>
    public void Update(float deltaTime)
    {
        _cooldownManager.Update(deltaTime);
    }

    /// <summary>
    /// Attempt to cast a spell from a specific slot
    /// </summary>
    public CastResult TryCastSpell(int slotIndex, Character caster, TrainingDummy target, Vector3 casterPosition)
    {
        // Check if we have a spell in this slot
        if (!_equippedSpells.TryGetValue(slotIndex, out SpellData spell))
        {
            return new CastResult { Success = false, FailureReason = "No spell equipped in this slot" };
        }

        // Check if the caster's class matches the spell requirements
        if (caster.ResourceType != spell.RequiredResourceType)
        {
            return new CastResult { Success = false, FailureReason = "Wrong class for this spell" };
        }

        // Check cooldown
        if (_cooldownManager.IsOnCooldown(spell.Name))
        {
            return new CastResult { Success = false, FailureReason = "Spell is on cooldown" };
        }

        // Check resource cost
        if (caster.CurrentResource < spell.ResourceCost)
        {
            return new CastResult { Success = false, FailureReason = "Not enough resource" };
        }

        // Check if we need a target
        if (spell.TargetType == SpellTargetType.SingleTarget)
        {
            if (target == null || !target.IsAlive)
            {
                return new CastResult { Success = false, FailureReason = "No valid target" };
            }

            // Check range
            float distance = Vector3.Distance(casterPosition, target.Position);
            if (distance > spell.Range)
            {
                return new CastResult { Success = false, FailureReason = "Out of range" };
            }
        }

        // All checks passed, cast the spell
        caster.TryConsumeResource(spell.ResourceCost);
        _cooldownManager.StartCooldown(spell.Name, spell.CooldownDuration);

        // Execute spell effect
        if (spell.TargetType == SpellTargetType.SingleTarget && target != null)
        {
            target.TakeDamage(spell.Damage);
        }

        return new CastResult 
        { 
            Success = true, 
            SpellCast = spell,
            DamageDealt = spell.Damage
        };
    }

    /// <summary>
    /// Get the spell equipped in a specific slot
    /// </summary>
    public SpellData GetEquippedSpell(int slotIndex)
    {
        return _equippedSpells.TryGetValue(slotIndex, out SpellData spell) ? spell : null;
    }

    /// <summary>
    /// Check if a spell is on cooldown
    /// </summary>
    public bool IsSpellOnCooldown(int slotIndex)
    {
        var spell = GetEquippedSpell(slotIndex);
        return spell != null && _cooldownManager.IsOnCooldown(spell.Name);
    }

    /// <summary>
    /// Get cooldown progress for a spell (0.0 = ready, 1.0 = just cast)
    /// </summary>
    public float GetSpellCooldownProgress(int slotIndex)
    {
        var spell = GetEquippedSpell(slotIndex);
        if (spell == null) return 0f;
        
        return _cooldownManager.GetCooldownProgress(spell.Name, spell.CooldownDuration);
    }

    /// <summary>
    /// Get remaining cooldown time for a spell
    /// </summary>
    public float GetSpellRemainingCooldown(int slotIndex)
    {
        var spell = GetEquippedSpell(slotIndex);
        if (spell == null) return 0f;
          return _cooldownManager.GetRemainingCooldown(spell.Name);
    }
}
