namespace project_axiom.UI;

/// <summary>
/// Manages temporary UI messages like "Out of Range"
/// </summary>
public class MessageDisplay
{
    private List<TemporaryMessage> _messages = new List<TemporaryMessage>();

    /// <summary>
    /// Update all messages and remove expired ones
    /// </summary>
    public void Update(float deltaTime)
    {
        for (int i = _messages.Count - 1; i >= 0; i--)
        {
            _messages[i].TimeRemaining -= deltaTime;
            if (_messages[i].TimeRemaining <= 0)
            {
                _messages.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Add a temporary message
    /// </summary>
    public void AddMessage(string text, float duration, Color color, MessagePosition position = MessagePosition.Center)
    {
        _messages.Add(new TemporaryMessage
        {
            Text = text,
            TimeRemaining = duration,
            TotalDuration = duration,
            Color = color,
            Position = position
        });
    }

    /// <summary>
    /// Add an "Out of Range" message
    /// </summary>
    public void ShowOutOfRangeMessage()
    {
        AddMessage("Out of Range!", 2.0f, Color.Red, MessagePosition.Center);
    }

    /// <summary>
    /// Add an "On Cooldown" message
    /// </summary>
    public void ShowOnCooldownMessage()
    {
        AddMessage("On Cooldown!", 2.0f, Color.Orange, MessagePosition.Center);
    }

    /// <summary>
    /// Add a "Not Enough [Resource]" message
    /// </summary>
    public void ShowNotEnoughResourceMessage(string resourceType)
    {
        AddMessage($"Not Enough {resourceType}!", 2.0f, Color.Blue, MessagePosition.Center);
    }

    /// <summary>
    /// Get all current messages
    /// </summary>
    public List<TemporaryMessage> GetMessages()
    {
        return new List<TemporaryMessage>(_messages);
    }

    /// <summary>
    /// Clear all messages
    /// </summary>
    public void Clear()
    {
        _messages.Clear();
    }
}

/// <summary>
/// Represents a temporary message to be displayed on screen
/// </summary>
public class TemporaryMessage
{
    public string Text { get; set; }
    public float TimeRemaining { get; set; }
    public float TotalDuration { get; set; }
    public Color Color { get; set; }
    public MessagePosition Position { get; set; }

    /// <summary>
    /// Get the alpha value based on time remaining (fades out in last 0.5 seconds)
    /// </summary>
    public float GetAlpha()
    {
        if (TimeRemaining <= 0.5f)
        {
            return TimeRemaining / 0.5f; // Fade out
        }
        return 1.0f;
    }
}

/// <summary>
/// Defines where messages should be displayed on screen
/// </summary>
public enum MessagePosition
{
    Center,
    Top,
    Bottom,
    Left,
    Right
}
