namespace Models;

/// <summary>
/// Represents an action item extracted from text
/// </summary>
public class ActionItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Description { get; set; } = string.Empty;
    public string? Owner { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Priority { get; set; }
    public string? Status { get; set; } = "Open";
    public string? Source { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
