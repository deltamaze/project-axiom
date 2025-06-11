namespace project_axiom.UI;

/// <summary>
/// Manages the visual state of spell bar slots
/// </summary>
public class SpellBarState
{
    private Dictionary<int, SlotState> _slotStates = new Dictionary<int, SlotState>();
    private Dictionary<int, float> _flashTimers = new Dictionary<int, float>();
    
    public const float FLASH_DURATION = 0.3f; // Duration of flash effect

    public SpellBarState()
    {
        // Initialize all 8 slots as ready
        for (int i = 0; i < 8; i++)
        {
            _slotStates[i] = SlotState.Ready;
            _flashTimers[i] = 0f;
        }
    }

    /// <summary>
    /// Update visual effects for all slots
    /// </summary>
    public void Update(float deltaTime)
    {
        for (int i = 0; i < 8; i++)
        {
            if (_flashTimers[i] > 0)
            {
                _flashTimers[i] -= deltaTime;
                if (_flashTimers[i] <= 0)
                {
                    _flashTimers[i] = 0f;
                    // If flash is over and slot is still flashing, set to ready
                    if (_slotStates[i] == SlotState.Flashing)
                    {
                        _slotStates[i] = SlotState.Ready;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Trigger a flash effect for a slot
    /// </summary>
    public void FlashSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < 8)
        {
            _slotStates[slotIndex] = SlotState.Flashing;
            _flashTimers[slotIndex] = FLASH_DURATION;
        }
    }

    /// <summary>
    /// Set a slot to cooldown state
    /// </summary>
    public void SetSlotOnCooldown(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < 8)
        {
            _slotStates[slotIndex] = SlotState.OnCooldown;
            _flashTimers[slotIndex] = 0f;
        }
    }

    /// <summary>
    /// Set a slot to ready state
    /// </summary>
    public void SetSlotReady(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < 8)
        {
            _slotStates[slotIndex] = SlotState.Ready;
            _flashTimers[slotIndex] = 0f;
        }
    }

    /// <summary>
    /// Get the current state of a slot
    /// </summary>
    public SlotState GetSlotState(int slotIndex)
    {
        return _slotStates.TryGetValue(slotIndex, out SlotState state) ? state : SlotState.Ready;
    }

    /// <summary>
    /// Get the flash intensity for a slot (0.0 to 1.0)
    /// </summary>
    public float GetFlashIntensity(int slotIndex)
    {
        if (_slotStates[slotIndex] != SlotState.Flashing) return 0f;
        return _flashTimers[slotIndex] / FLASH_DURATION;
    }
}

/// <summary>
/// Represents the visual state of a spell bar slot
/// </summary>
public enum SlotState
{
    Ready,
    Flashing,
    OnCooldown
}
