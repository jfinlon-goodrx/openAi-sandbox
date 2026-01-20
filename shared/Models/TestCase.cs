namespace Models;

/// <summary>
/// Represents a test case
/// </summary>
public class TestCase
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? TestType { get; set; } // Unit, Integration, E2E
    public List<string> Steps { get; set; } = new();
    public string? ExpectedResult { get; set; }
    public string? ActualResult { get; set; }
    public string? Status { get; set; } = "Not Run";
    public List<string> Tags { get; set; } = new();
    public string? SourceCode { get; set; }
    public string? SourceUserStory { get; set; }
}
