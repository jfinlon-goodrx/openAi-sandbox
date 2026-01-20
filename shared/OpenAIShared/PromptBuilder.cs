using System.Text;

namespace OpenAIShared;

/// <summary>
/// Helper class for building structured prompts
/// </summary>
public class PromptBuilder
{
    private readonly StringBuilder _sb = new();
    private readonly List<string> _examples = new();
    private string? _systemMessage;
    private string? _context;

    /// <summary>
    /// Sets the system message/instruction
    /// </summary>
    public PromptBuilder WithSystemMessage(string message)
    {
        _systemMessage = message;
        return this;
    }

    /// <summary>
    /// Adds context information
    /// </summary>
    public PromptBuilder WithContext(string context)
    {
        _context = context;
        return this;
    }

    /// <summary>
    /// Adds a few-shot example
    /// </summary>
    public PromptBuilder WithExample(string input, string output)
    {
        _examples.Add($"Input: {input}\nOutput: {output}");
        return this;
    }

    /// <summary>
    /// Adds user instruction
    /// </summary>
    public PromptBuilder WithInstruction(string instruction)
    {
        _sb.AppendLine(instruction);
        return this;
    }

    /// <summary>
    /// Adds user input/data
    /// </summary>
    public PromptBuilder WithInput(string input)
    {
        _sb.AppendLine(input);
        return this;
    }

    /// <summary>
    /// Builds the complete prompt
    /// </summary>
    public string Build()
    {
        var result = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(_systemMessage))
        {
            result.AppendLine(_systemMessage);
            result.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(_context))
        {
            result.AppendLine("Context:");
            result.AppendLine(_context);
            result.AppendLine();
        }

        if (_examples.Count > 0)
        {
            result.AppendLine("Examples:");
            foreach (var example in _examples)
            {
                result.AppendLine(example);
                result.AppendLine();
            }
        }

        result.Append(_sb.ToString());

        return result.ToString().Trim();
    }

    /// <summary>
    /// Clears the builder
    /// </summary>
    public PromptBuilder Clear()
    {
        _sb.Clear();
        _examples.Clear();
        _systemMessage = null;
        _context = null;
        return this;
    }
}
