using System.Text.Json;
using Microsoft.Extensions.Logging;
using OpenAIShared;
using Markdig;

namespace PublishingAssistant.Core;

/// <summary>
/// Service for publishing-related AI tasks: reviews, summaries, marketing blurbs, and content generation
/// </summary>
public class PublishingService
{
    private readonly OpenAIClient _openAIClient;
    private readonly ILogger<PublishingService> _logger;
    private readonly string _model;

    public PublishingService(
        OpenAIClient openAIClient,
        ILogger<PublishingService> logger,
        string model = "gpt-4-turbo-preview")
    {
        _openAIClient = openAIClient;
        _logger = logger;
        _model = model;
    }

    /// <summary>
    /// Generates a comprehensive book review
    /// </summary>
    public async Task<BookReview> GenerateReviewAsync(
        string manuscriptContent,
        string? genre = null,
        CancellationToken cancellationToken = default)
    {
        var functionDefinition = new FunctionDefinition
        {
            Name = "generate_book_review",
            Description = "Generates a comprehensive book review",
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    overallRating = new { type = "number", description = "Rating from 1 to 5" },
                    strengths = new
                    {
                        type = "array",
                        items = new { type = "string" },
                        description = "Key strengths of the manuscript"
                    },
                    weaknesses = new
                    {
                        type = "array",
                        items = new { type = "string" },
                        description = "Areas for improvement"
                    },
                    plotSummary = new { type = "string", description = "Brief plot summary" },
                    characterAnalysis = new { type = "string", description = "Character development analysis" },
                    writingStyle = new { type = "string", description = "Writing style assessment" },
                    recommendations = new { type = "string", description = "Editorial recommendations" },
                    targetAudience = new { type = "string", description = "Recommended target audience" }
                },
                required = new[] { "overallRating", "strengths", "plotSummary", "recommendations" }
            }
        };

        var genreContext = !string.IsNullOrEmpty(genre) ? $"Genre: {genre}. " : "";
        var prompt = new PromptBuilder()
            .WithSystemMessage("You are an expert book editor and literary critic with decades of experience.")
            .WithInstruction($"{genreContext}Review the following manuscript. Provide a comprehensive analysis including plot, characters, writing style, strengths, weaknesses, and editorial recommendations.")
            .WithInput(manuscriptContent)
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = "You are an expert book editor and literary critic." },
                new() { Role = "user", Content = prompt }
            },
            Functions = new List<FunctionDefinition> { functionDefinition },
            FunctionCall = "auto",
            Temperature = 0.4,
            MaxTokens = 3000
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        var message = response.Choices.FirstOrDefault()?.Message;

        if (message?.FunctionCall != null)
        {
            return ParseReviewFromFunctionCall(message.FunctionCall.Arguments);
        }

        return new BookReview
        {
            OverallRating = 0,
            PlotSummary = message?.Content ?? "Unable to generate review."
        };
    }

    /// <summary>
    /// Generates a book summary (blurb) for back cover or marketing
    /// </summary>
    public async Task<string> GenerateSummaryAsync(
        string manuscriptContent,
        int maxLength = 250,
        CancellationToken cancellationToken = default)
    {
        var prompt = new PromptBuilder()
            .WithSystemMessage("You are an expert copywriter specializing in book summaries and blurbs.")
            .WithInstruction($"Generate a compelling book summary (blurb) of approximately {maxLength} words. " +
                           "Make it engaging, avoid spoilers, and highlight what makes this book unique. " +
                           "Write in third person, present tense.")
            .WithInput(manuscriptContent)
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = "You are an expert copywriter specializing in book summaries." },
                new() { Role = "user", Content = prompt }
            },
            Temperature = 0.7,
            MaxTokens = 500
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        return response.Choices.FirstOrDefault()?.Message?.Content ?? "Unable to generate summary.";
    }

    /// <summary>
    /// Generates marketing copy/blurb for promotional materials
    /// </summary>
    public async Task<MarketingBlurb> GenerateMarketingBlurbAsync(
        string manuscriptContent,
        string? targetAudience = null,
        CancellationToken cancellationToken = default)
    {
        var functionDefinition = new FunctionDefinition
        {
            Name = "generate_marketing_blurb",
            Description = "Generates marketing copy for book promotion",
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    headline = new { type = "string", description = "Compelling headline" },
                    shortBlurb = new { type = "string", description = "Short blurb (50-100 words)" },
                    longBlurb = new { type = "string", description = "Long blurb (200-300 words)" },
                    tagline = new { type = "string", description = "One-line tagline" },
                    keySellingPoints = new
                    {
                        type = "array",
                        items = new { type = "string" },
                        description = "3-5 key selling points"
                    },
                    socialMediaPosts = new
                    {
                        type = "object",
                        properties = new
                        {
                            twitter = new { type = "string" },
                            facebook = new { type = "string" },
                            instagram = new { type = "string" }
                        }
                    }
                },
                required = new[] { "headline", "shortBlurb", "tagline", "keySellingPoints" }
            }
        };

        var audienceContext = !string.IsNullOrEmpty(targetAudience) ? $"Target audience: {targetAudience}. " : "";
        var prompt = new PromptBuilder()
            .WithSystemMessage("You are an expert marketing copywriter specializing in book promotion.")
            .WithInstruction($"{audienceContext}Create compelling marketing copy including headlines, blurbs, taglines, and social media posts. " +
                           "Make it engaging and designed to drive sales.")
            .WithInput(manuscriptContent)
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = "You are an expert marketing copywriter." },
                new() { Role = "user", Content = prompt }
            },
            Functions = new List<FunctionDefinition> { functionDefinition },
            FunctionCall = "auto",
            Temperature = 0.8,
            MaxTokens = 2000
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        var message = response.Choices.FirstOrDefault()?.Message;

        if (message?.FunctionCall != null)
        {
            return ParseMarketingBlurbFromFunctionCall(message.FunctionCall.Arguments);
        }

        return new MarketingBlurb
        {
            Headline = "New Release",
            ShortBlurb = message?.Content ?? "Unable to generate marketing blurb."
        };
    }

    /// <summary>
    /// Generates a cover image description based on manuscript content
    /// </summary>
    public async Task<CoverImageDescription> GenerateCoverImageDescriptionAsync(
        string manuscriptContent,
        string? genre = null,
        CancellationToken cancellationToken = default)
    {
        var functionDefinition = new FunctionDefinition
        {
            Name = "generate_cover_description",
            Description = "Generates detailed cover image description for DALL-E or other image generators",
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    description = new { type = "string", description = "Detailed image description for DALL-E" },
                    style = new { type = "string", description = "Artistic style (e.g., minimalist, photorealistic, illustrated)" },
                    colorPalette = new
                    {
                        type = "array",
                        items = new { type = "string" },
                        description = "Recommended color palette"
                    },
                    mood = new { type = "string", description = "Overall mood/atmosphere" },
                    keyElements = new
                    {
                        type = "array",
                        items = new { type = "string" },
                        description = "Key visual elements to include"
                    },
                    typography = new { type = "string", description = "Typography recommendations" }
                },
                required = new[] { "description", "style", "colorPalette", "mood" }
            }
        };

        var genreContext = !string.IsNullOrEmpty(genre) ? $"Genre: {genre}. " : "";
        var prompt = new PromptBuilder()
            .WithSystemMessage("You are an expert book cover designer and art director.")
            .WithInstruction($"{genreContext}Analyze the manuscript and create a detailed cover image description suitable for DALL-E or other AI image generators. " +
                           "Include style, colors, mood, and key visual elements. Make it compelling and marketable.")
            .WithInput(manuscriptContent)
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = "You are an expert book cover designer." },
                new() { Role = "user", Content = prompt }
            },
            Functions = new List<FunctionDefinition> { functionDefinition },
            FunctionCall = "auto",
            Temperature = 0.6,
            MaxTokens = 1500
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        var message = response.Choices.FirstOrDefault()?.Message;

        if (message?.FunctionCall != null)
        {
            return ParseCoverDescriptionFromFunctionCall(message.FunctionCall.Arguments);
        }

        return new CoverImageDescription
        {
            Description = message?.Content ?? "Unable to generate cover description.",
            Style = "Modern",
            Mood = "Engaging"
        };
    }

    /// <summary>
    /// Converts markdown content to various formats
    /// </summary>
    public async Task<string> ConvertMarkdownToFormatAsync(
        string markdownContent,
        string targetFormat,
        CancellationToken cancellationToken = default)
    {
        targetFormat = targetFormat.ToLowerInvariant();

        return targetFormat switch
        {
            "html" => Markdown.ToHtml(markdownContent),
            "plaintext" => Markdown.ToPlainText(markdownContent),
            "epub" => await ConvertToEpubAsync(markdownContent, cancellationToken),
            "pdf" => await ConvertToPdfAsync(markdownContent, cancellationToken),
            _ => throw new NotSupportedException($"Format {targetFormat} is not supported")
        };
    }

    /// <summary>
    /// Generates editorial notes and suggestions
    /// </summary>
    public async Task<string> GenerateEditorialNotesAsync(
        string manuscriptContent,
        CancellationToken cancellationToken = default)
    {
        var prompt = new PromptBuilder()
            .WithSystemMessage("You are an experienced book editor. Provide detailed editorial notes and suggestions.")
            .WithInstruction("Review the manuscript and provide editorial notes covering:\n" +
                           "1. Structure and pacing\n" +
                           "2. Character development\n" +
                           "3. Dialogue quality\n" +
                           "4. Show vs. tell issues\n" +
                           "5. Consistency issues\n" +
                           "6. Grammar and style\n" +
                           "7. Overall recommendations")
            .WithInput(manuscriptContent)
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = "You are an experienced book editor." },
                new() { Role = "user", Content = prompt }
            },
            Temperature = 0.3,
            MaxTokens = 3000
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        return response.Choices.FirstOrDefault()?.Message?.Content ?? "Unable to generate editorial notes.";
    }

    private async Task<string> ConvertToEpubAsync(string markdownContent, CancellationToken cancellationToken)
    {
        // Convert markdown to HTML first, then provide instructions for EPUB conversion
        var html = Markdown.ToHtml(markdownContent);
        
        var prompt = new PromptBuilder()
            .WithSystemMessage("You are an expert in EPUB format conversion.")
            .WithInstruction("Convert the following HTML content to EPUB format structure. Provide the EPUB XML structure.")
            .WithInput(html)
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "user", Content = prompt }
            },
            Temperature = 0.2,
            MaxTokens = 5000
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        return response.Choices.FirstOrDefault()?.Message?.Content ?? html;
    }

    private async Task<string> ConvertToPdfAsync(string markdownContent, CancellationToken cancellationToken)
    {
        // Convert to HTML first, then provide PDF conversion instructions
        var html = Markdown.ToHtml(markdownContent);
        return html; // In production, use a library like Puppeteer or wkhtmltopdf
    }

    private BookReview ParseReviewFromFunctionCall(string argumentsJson)
    {
        try
        {
            var arguments = JsonSerializer.Deserialize<JsonElement>(argumentsJson);
            var review = new BookReview
            {
                OverallRating = arguments.GetProperty("overallRating").GetDouble(),
                PlotSummary = arguments.GetProperty("plotSummary").GetString() ?? string.Empty
            };

            if (arguments.TryGetProperty("strengths", out var strengths))
            {
                review.Strengths = strengths.EnumerateArray()
                    .Select(s => s.GetString() ?? string.Empty)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
            }

            if (arguments.TryGetProperty("weaknesses", out var weaknesses))
            {
                review.Weaknesses = weaknesses.EnumerateArray()
                    .Select(w => w.GetString() ?? string.Empty)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
            }

            if (arguments.TryGetProperty("characterAnalysis", out var character))
                review.CharacterAnalysis = character.GetString();
            if (arguments.TryGetProperty("writingStyle", out var style))
                review.WritingStyle = style.GetString();
            if (arguments.TryGetProperty("recommendations", out var recommendations))
                review.Recommendations = recommendations.GetString();
            if (arguments.TryGetProperty("targetAudience", out var audience))
                review.TargetAudience = audience.GetString();

            return review;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing review from function call");
            return new BookReview { OverallRating = 0, PlotSummary = "Error parsing review." };
        }
    }

    private MarketingBlurb ParseMarketingBlurbFromFunctionCall(string argumentsJson)
    {
        try
        {
            var arguments = JsonSerializer.Deserialize<JsonElement>(argumentsJson);
            var blurb = new MarketingBlurb
            {
                Headline = arguments.GetProperty("headline").GetString() ?? string.Empty,
                ShortBlurb = arguments.GetProperty("shortBlurb").GetString() ?? string.Empty,
                Tagline = arguments.GetProperty("tagline").GetString() ?? string.Empty
            };

            if (arguments.TryGetProperty("longBlurb", out var longBlurb))
                blurb.LongBlurb = longBlurb.GetString();

            if (arguments.TryGetProperty("keySellingPoints", out var points))
            {
                blurb.KeySellingPoints = points.EnumerateArray()
                    .Select(p => p.GetString() ?? string.Empty)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
            }

            if (arguments.TryGetProperty("socialMediaPosts", out var social))
            {
                blurb.SocialMediaPosts = new SocialMediaPosts();
                if (social.TryGetProperty("twitter", out var twitter))
                    blurb.SocialMediaPosts.Twitter = twitter.GetString();
                if (social.TryGetProperty("facebook", out var facebook))
                    blurb.SocialMediaPosts.Facebook = facebook.GetString();
                if (social.TryGetProperty("instagram", out var instagram))
                    blurb.SocialMediaPosts.Instagram = instagram.GetString();
            }

            return blurb;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing marketing blurb from function call");
            return new MarketingBlurb { Headline = "New Release", ShortBlurb = "Error parsing blurb." };
        }
    }

    private CoverImageDescription ParseCoverDescriptionFromFunctionCall(string argumentsJson)
    {
        try
        {
            var arguments = JsonSerializer.Deserialize<JsonElement>(argumentsJson);
            var description = new CoverImageDescription
            {
                Description = arguments.GetProperty("description").GetString() ?? string.Empty,
                Style = arguments.GetProperty("style").GetString() ?? "Modern",
                Mood = arguments.GetProperty("mood").GetString() ?? "Engaging"
            };

            if (arguments.TryGetProperty("colorPalette", out var colors))
            {
                description.ColorPalette = colors.EnumerateArray()
                    .Select(c => c.GetString() ?? string.Empty)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
            }

            if (arguments.TryGetProperty("keyElements", out var elements))
            {
                description.KeyElements = elements.EnumerateArray()
                    .Select(e => e.GetString() ?? string.Empty)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
            }

            if (arguments.TryGetProperty("typography", out var typography))
                description.Typography = typography.GetString();

            return description;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing cover description from function call");
            return new CoverImageDescription
            {
                Description = "Error parsing description.",
                Style = "Modern",
                Mood = "Engaging"
            };
        }
    }
}

// Data models
public class BookReview
{
    public double OverallRating { get; set; }
    public string PlotSummary { get; set; } = string.Empty;
    public List<string> Strengths { get; set; } = new();
    public List<string> Weaknesses { get; set; } = new();
    public string? CharacterAnalysis { get; set; }
    public string? WritingStyle { get; set; }
    public string? Recommendations { get; set; }
    public string? TargetAudience { get; set; }
}

public class MarketingBlurb
{
    public string Headline { get; set; } = string.Empty;
    public string ShortBlurb { get; set; } = string.Empty;
    public string? LongBlurb { get; set; }
    public string Tagline { get; set; } = string.Empty;
    public List<string> KeySellingPoints { get; set; } = new();
    public SocialMediaPosts? SocialMediaPosts { get; set; }
}

public class SocialMediaPosts
{
    public string? Twitter { get; set; }
    public string? Facebook { get; set; }
    public string? Instagram { get; set; }
}

public class CoverImageDescription
{
    public string Description { get; set; } = string.Empty;
    public string Style { get; set; } = string.Empty;
    public string Mood { get; set; } = string.Empty;
    public List<string> ColorPalette { get; set; } = new();
    public List<string> KeyElements { get; set; } = new();
    public string? Typography { get; set; }
}
