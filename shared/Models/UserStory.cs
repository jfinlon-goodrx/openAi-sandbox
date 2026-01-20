namespace Models;

/// <summary>
/// Represents a user story with acceptance criteria
/// </summary>
public class UserStory
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AsA { get; set; } = string.Empty;
    public string IWant { get; set; } = string.Empty;
    public string SoThat { get; set; } = string.Empty;
    public List<string> AcceptanceCriteria { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public string? Priority { get; set; }
    public string? SourceDocument { get; set; }
}
