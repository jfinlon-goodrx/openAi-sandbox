using System.Text.Json;
using Microsoft.Extensions.Logging;
using OpenAIShared;

namespace AdvertisingAgency.Core;

/// <summary>
/// Service for advertising agency AI tasks: ad copy, campaign strategy, audience analysis, and creative briefs
/// </summary>
public class AdvertisingService
{
    private readonly OpenAIClient _openAIClient;
    private readonly ILogger<AdvertisingService> _logger;
    private readonly string _model;

    public AdvertisingService(
        OpenAIClient openAIClient,
        ILogger<AdvertisingService> logger,
        string model = "gpt-4-turbo-preview")
    {
        _openAIClient = openAIClient;
        _logger = logger;
        _model = model;
    }

    /// <summary>
    /// Generates ad copy for various channels
    /// </summary>
    public async Task<AdCopy> GenerateAdCopyAsync(
        string productName,
        string productDescription,
        string targetAudience,
        string channel,
        string? tone = null,
        CancellationToken cancellationToken = default)
    {
        var functionDefinition = new FunctionDefinition
        {
            Name = "generate_ad_copy",
            Description = "Generates advertising copy for various channels",
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    headline = new { type = "string" },
                    subheadline = new { type = "string" },
                    bodyCopy = new { type = "string" },
                    callToAction = new { type = "string" },
                    tagline = new { type = "string" },
                    socialMediaPosts = new
                    {
                        type = "object",
                        properties = new
                        {
                            twitter = new { type = "string" },
                            facebook = new { type = "string" },
                            instagram = new { type = "string" },
                            linkedin = new { type = "string" }
                        }
                    },
                    keyMessages = new
                    {
                        type = "array",
                        items = new { type = "string" }
                    }
                },
                required = new[] { "headline", "bodyCopy", "callToAction" }
            }
        };

        var toneContext = !string.IsNullOrEmpty(tone) ? $"Tone: {tone}. " : "";
        var prompt = new PromptBuilder()
            .WithSystemMessage("You are an expert copywriter at a top advertising agency.")
            .WithInstruction($"Create compelling ad copy for {productName}. " +
                           $"Product: {productDescription}. " +
                           $"Target Audience: {targetAudience}. " +
                           $"Channel: {channel}. " +
                           $"{toneContext}" +
                           "Generate headline, subheadline, body copy, call-to-action, tagline, social media posts, and key messages.")
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = "You are an expert copywriter at a top advertising agency." },
                new() { Role = "user", Content = prompt }
            },
            Functions = new List<FunctionDefinition> { functionDefinition },
            FunctionCall = "auto",
            Temperature = 0.7,
            MaxTokens = 2500
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        var message = response.Choices.FirstOrDefault()?.Message;

        if (message?.FunctionCall != null)
        {
            return ParseAdCopyFromFunctionCall(message.FunctionCall.Arguments, productName, channel);
        }

        return new AdCopy
        {
            ProductName = productName,
            Channel = channel,
            Headline = message?.Content ?? "Unable to generate ad copy."
        };
    }

    /// <summary>
    /// Develops campaign strategy
    /// </summary>
    public async Task<CampaignStrategy> DevelopCampaignStrategyAsync(
        string brandName,
        string productDescription,
        string targetAudience,
        string campaignObjective,
        decimal? budget = null,
        CancellationToken cancellationToken = default)
    {
        var functionDefinition = new FunctionDefinition
        {
            Name = "develop_campaign_strategy",
            Description = "Develops comprehensive advertising campaign strategy",
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    campaignTheme = new { type = "string" },
                    keyMessages = new
                    {
                        type = "array",
                        items = new { type = "string" }
                    },
                    channels = new
                    {
                        type = "array",
                        items = new
                        {
                            type = "object",
                            properties = new
                            {
                                channel = new { type = "string" },
                                rationale = new { type = "string" },
                                budgetAllocation = new { type = "string" }
                            }
                        }
                    },
                    targetAudienceSegments = new
                    {
                        type = "array",
                        items = new
                        {
                            type = "object",
                            properties = new
                            {
                                segment = new { type = "string" },
                                characteristics = new { type = "string" },
                                messaging = new { type = "string" }
                            }
                        }
                    },
                    timeline = new
                    {
                        type = "object",
                        properties = new
                        {
                            phases = new
                            {
                                type = "array",
                                items = new
                                {
                                    type = "object",
                                    properties = new
                                    {
                                        phase = new { type = "string" },
                                        duration = new { type = "string" },
                                        activities = new
                                        {
                                            type = "array",
                                            items = new { type = "string" }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    successMetrics = new
                    {
                        type = "array",
                        items = new { type = "string" }
                    }
                },
                required = new[] { "campaignTheme", "keyMessages", "channels" }
            }
        };

        var budgetContext = budget.HasValue ? $"Budget: ${budget:N0}. " : "";
        var prompt = new PromptBuilder()
            .WithSystemMessage("You are a strategic planner at a top advertising agency.")
            .WithInstruction($"Develop a comprehensive campaign strategy for {brandName}. " +
                           $"Product: {productDescription}. " +
                           $"Target Audience: {targetAudience}. " +
                           $"Objective: {campaignObjective}. " +
                           $"{budgetContext}" +
                           "Include campaign theme, key messages, channel strategy, audience segments, timeline, and success metrics.")
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = "You are a strategic planner at a top advertising agency." },
                new() { Role = "user", Content = prompt }
            },
            Functions = new List<FunctionDefinition> { functionDefinition },
            FunctionCall = "auto",
            Temperature = 0.6,
            MaxTokens = 4000
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        var message = response.Choices.FirstOrDefault()?.Message;

        if (message?.FunctionCall != null)
        {
            return ParseCampaignStrategyFromFunctionCall(message.FunctionCall.Arguments, brandName, campaignObjective);
        }

        return new CampaignStrategy
        {
            BrandName = brandName,
            CampaignObjective = campaignObjective,
            CampaignTheme = message?.Content ?? "Unable to generate strategy."
        };
    }

    /// <summary>
    /// Analyzes target audience
    /// </summary>
    public async Task<TargetAudienceAnalysis> AnalyzeTargetAudienceAsync(
        string productDescription,
        string? demographic = null,
        string? psychographic = null,
        CancellationToken cancellationToken = default)
    {
        var functionDefinition = new FunctionDefinition
        {
            Name = "analyze_target_audience",
            Description = "Analyzes target audience for advertising",
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    primaryAudience = new
                    {
                        type = "object",
                        properties = new
                        {
                            demographics = new { type = "string" },
                            psychographics = new { type = "string" },
                            painPoints = new
                            {
                                type = "array",
                                items = new { type = "string" }
                            },
                            motivations = new
                            {
                                type = "array",
                                items = new { type = "string" }
                            },
                            mediaConsumption = new { type = "string" }
                        }
                    },
                    secondaryAudiences = new
                    {
                        type = "array",
                        items = new { type = "string" }
                    },
                    messagingRecommendations = new
                    {
                        type = "array",
                        items = new { type = "string" }
                    },
                    channelRecommendations = new
                    {
                        type = "array",
                        items = new { type = "string" }
                    }
                },
                required = new[] { "primaryAudience", "messagingRecommendations" }
            }
        };

        var demoContext = !string.IsNullOrEmpty(demographic) ? $"Demographics: {demographic}. " : "";
        var psychoContext = !string.IsNullOrEmpty(psychographic) ? $"Psychographics: {psychographic}. " : "";
        var prompt = new PromptBuilder()
            .WithSystemMessage("You are a market research analyst at an advertising agency.")
            .WithInstruction($"Analyze the target audience for: {productDescription}. " +
                           $"{demoContext}{psychoContext}" +
                           "Provide demographics, psychographics, pain points, motivations, media consumption, " +
                           "messaging recommendations, and channel recommendations.")
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = "You are a market research analyst." },
                new() { Role = "user", Content = prompt }
            },
            Functions = new List<FunctionDefinition> { functionDefinition },
            FunctionCall = "auto",
            Temperature = 0.5,
            MaxTokens = 3000
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        var message = response.Choices.FirstOrDefault()?.Message;

        if (message?.FunctionCall != null)
        {
            return ParseTargetAudienceFromFunctionCall(message.FunctionCall.Arguments);
        }

        return new TargetAudienceAnalysis
        {
            PrimaryAudience = new AudienceProfile(),
            MessagingRecommendations = new List<string>()
        };
    }

    /// <summary>
    /// Develops brand voice and tone guidelines
    /// </summary>
    public async Task<BrandVoice> DevelopBrandVoiceAsync(
        string brandName,
        string brandDescription,
        string? existingContent = null,
        CancellationToken cancellationToken = default)
    {
        var functionDefinition = new FunctionDefinition
        {
            Name = "develop_brand_voice",
            Description = "Develops brand voice and tone guidelines",
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    voiceDescription = new { type = "string" },
                    toneGuidelines = new
                    {
                        type = "array",
                        items = new
                        {
                            type = "object",
                            properties = new
                            {
                                tone = new { type = "string" },
                                whenToUse = new { type = "string" },
                                examples = new
                                {
                                    type = "array",
                                    items = new { type = "string" }
                                }
                            }
                        }
                    },
                    doAndDonts = new
                    {
                        type = "object",
                        properties = new
                        {
                            do = new
                            {
                                type = "array",
                                items = new { type = "string" }
                            },
                            dont = new
                            {
                                type = "array",
                                items = new { type = "string" }
                            }
                        }
                    },
                    wordChoices = new
                    {
                        type = "object",
                        properties = new
                        {
                            preferred = new
                            {
                                type = "array",
                                items = new { type = "string" }
                            },
                            avoid = new
                            {
                                type = "array",
                                items = new { type = "string" }
                            }
                        }
                    }
                },
                required = new[] { "voiceDescription", "toneGuidelines" }
            }
        };

        var existingContext = !string.IsNullOrEmpty(existingContent) ? $"Existing content examples:\n{existingContent}\n\n" : "";
        var prompt = new PromptBuilder()
            .WithSystemMessage("You are a brand strategist specializing in brand voice development.")
            .WithInstruction($"Develop brand voice and tone guidelines for {brandName}. " +
                           $"Brand description: {brandDescription}. " +
                           $"{existingContext}" +
                           "Create comprehensive voice description, tone guidelines, do's and don'ts, and word choices.")
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = "You are a brand strategist." },
                new() { Role = "user", Content = prompt }
            },
            Functions = new List<FunctionDefinition> { functionDefinition },
            FunctionCall = "auto",
            Temperature = 0.5,
            MaxTokens = 3000
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        var message = response.Choices.FirstOrDefault()?.Message;

        if (message?.FunctionCall != null)
        {
            return ParseBrandVoiceFromFunctionCall(message.FunctionCall.Arguments, brandName);
        }

        return new BrandVoice
        {
            BrandName = brandName,
            VoiceDescription = message?.Content ?? "Unable to generate brand voice."
        };
    }

    /// <summary>
    /// Generates creative brief
    /// </summary>
    public async Task<CreativeBrief> GenerateCreativeBriefAsync(
        string clientName,
        string productDescription,
        string campaignObjective,
        string targetAudience,
        string? budget = null,
        CancellationToken cancellationToken = default)
    {
        var functionDefinition = new FunctionDefinition
        {
            Name = "generate_creative_brief",
            Description = "Generates comprehensive creative brief",
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    background = new { type = "string" },
                    objective = new { type = "string" },
                    targetAudience = new { type = "string" },
                    keyMessage = new { type = "string" },
                    toneOfVoice = new { type = "string" },
                    deliverables = new
                    {
                        type = "array",
                        items = new { type = "string" }
                    },
                    constraints = new
                    {
                        type = "array",
                        items = new { type = "string" }
                    },
                    successCriteria = new
                    {
                        type = "array",
                        items = new { type = "string" }
                    },
                    timeline = new { type = "string" }
                },
                required = new[] { "background", "objective", "targetAudience", "keyMessage" }
            }
        };

        var budgetContext = !string.IsNullOrEmpty(budget) ? $"Budget: {budget}. " : "";
        var prompt = new PromptBuilder()
            .WithSystemMessage("You are a creative director at a top advertising agency.")
            .WithInstruction($"Create a comprehensive creative brief for {clientName}. " +
                           $"Product: {productDescription}. " +
                           $"Objective: {campaignObjective}. " +
                           $"Target Audience: {targetAudience}. " +
                           $"{budgetContext}" +
                           "Include background, objective, target audience, key message, tone, deliverables, constraints, success criteria, and timeline.")
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = "You are a creative director." },
                new() { Role = "user", Content = prompt }
            },
            Functions = new List<FunctionDefinition> { functionDefinition },
            FunctionCall = "auto",
            Temperature = 0.5,
            MaxTokens = 3000
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        var message = response.Choices.FirstOrDefault()?.Message;

        if (message?.FunctionCall != null)
        {
            return ParseCreativeBriefFromFunctionCall(message.FunctionCall.Arguments, clientName);
        }

        return new CreativeBrief
        {
            ClientName = clientName,
            Background = message?.Content ?? "Unable to generate creative brief."
        };
    }

    /// <summary>
    /// Generates A/B test hypotheses
    /// </summary>
    public async Task<List<ABTestHypothesis>> GenerateABTestHypothesesAsync(
        string campaignDescription,
        string metric,
        CancellationToken cancellationToken = default)
    {
        var functionDefinition = new FunctionDefinition
        {
            Name = "generate_ab_test_hypotheses",
            Description = "Generates A/B test hypotheses",
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    hypotheses = new
                    {
                        type = "array",
                        items = new
                        {
                            type = "object",
                            properties = new
                            {
                                hypothesis = new { type = "string" },
                                variantA = new { type = "string" },
                                variantB = new { type = "string" },
                                expectedOutcome = new { type = "string" },
                                rationale = new { type = "string" }
                            },
                            required = new[] { "hypothesis", "variantA", "variantB", "expectedOutcome" }
                        }
                    }
                },
                required = new[] { "hypotheses" }
            }
        };

        var prompt = new PromptBuilder()
            .WithSystemMessage("You are a data-driven marketing strategist specializing in A/B testing.")
            .WithInstruction($"Generate A/B test hypotheses for: {campaignDescription}. " +
                           $"Primary metric: {metric}. " +
                           "Create multiple testable hypotheses with clear variants, expected outcomes, and rationale.")
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = "You are a data-driven marketing strategist." },
                new() { Role = "user", Content = prompt }
            },
            Functions = new List<FunctionDefinition> { functionDefinition },
            FunctionCall = "auto",
            Temperature = 0.6,
            MaxTokens = 2500
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        var message = response.Choices.FirstOrDefault()?.Message;

        if (message?.FunctionCall != null)
        {
            return ParseABTestHypothesesFromFunctionCall(message.FunctionCall.Arguments);
        }

        return new List<ABTestHypothesis>();
    }

    // Parsing methods
    private AdCopy ParseAdCopyFromFunctionCall(string argumentsJson, string productName, string channel)
    {
        try
        {
            var arguments = JsonSerializer.Deserialize<JsonElement>(argumentsJson);
            var adCopy = new AdCopy
            {
                ProductName = productName,
                Channel = channel,
                Headline = arguments.GetProperty("headline").GetString() ?? string.Empty,
                BodyCopy = arguments.GetProperty("bodyCopy").GetString() ?? string.Empty,
                CallToAction = arguments.GetProperty("callToAction").GetString() ?? string.Empty
            };

            if (arguments.TryGetProperty("subheadline", out var subheadline))
                adCopy.Subheadline = subheadline.GetString();

            if (arguments.TryGetProperty("tagline", out var tagline))
                adCopy.Tagline = tagline.GetString();

            if (arguments.TryGetProperty("socialMediaPosts", out var social))
            {
                adCopy.SocialMediaPosts = new SocialMediaContent();
                if (social.TryGetProperty("twitter", out var twitter))
                    adCopy.SocialMediaPosts.Twitter = twitter.GetString();
                if (social.TryGetProperty("facebook", out var facebook))
                    adCopy.SocialMediaPosts.Facebook = facebook.GetString();
                if (social.TryGetProperty("instagram", out var instagram))
                    adCopy.SocialMediaPosts.Instagram = instagram.GetString();
                if (social.TryGetProperty("linkedin", out var linkedin))
                    adCopy.SocialMediaPosts.LinkedIn = linkedin.GetString();
            }

            if (arguments.TryGetProperty("keyMessages", out var messages))
            {
                adCopy.KeyMessages = messages.EnumerateArray()
                    .Select(m => m.GetString() ?? string.Empty)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
            }

            return adCopy;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing ad copy");
            return new AdCopy { ProductName = productName, Channel = channel, Headline = "Error parsing ad copy." };
        }
    }

    private CampaignStrategy ParseCampaignStrategyFromFunctionCall(string argumentsJson, string brandName, string objective)
    {
        try
        {
            var arguments = JsonSerializer.Deserialize<JsonElement>(argumentsJson);
            var strategy = new CampaignStrategy
            {
                BrandName = brandName,
                CampaignObjective = objective,
                CampaignTheme = arguments.GetProperty("campaignTheme").GetString() ?? string.Empty
            };

            if (arguments.TryGetProperty("keyMessages", out var messages))
            {
                strategy.KeyMessages = messages.EnumerateArray()
                    .Select(m => m.GetString() ?? string.Empty)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
            }

            if (arguments.TryGetProperty("channels", out var channels))
            {
                foreach (var channel in channels.EnumerateArray())
                {
                    strategy.Channels.Add(new ChannelStrategy
                    {
                        Channel = channel.GetProperty("channel").GetString() ?? string.Empty,
                        Rationale = channel.GetProperty("rationale").GetString() ?? string.Empty,
                        BudgetAllocation = channel.TryGetProperty("budgetAllocation", out var budget) ? budget.GetString() : null
                    });
                }
            }

            if (arguments.TryGetProperty("targetAudienceSegments", out var segments))
            {
                foreach (var segment in segments.EnumerateArray())
                {
                    strategy.TargetAudienceSegments.Add(new AudienceSegment
                    {
                        Segment = segment.GetProperty("segment").GetString() ?? string.Empty,
                        Characteristics = segment.GetProperty("characteristics").GetString() ?? string.Empty,
                        Messaging = segment.GetProperty("messaging").GetString() ?? string.Empty
                    });
                }
            }

            if (arguments.TryGetProperty("timeline", out var timeline) && timeline.TryGetProperty("phases", out var phases))
            {
                foreach (var phase in phases.EnumerateArray())
                {
                    var campaignPhase = new CampaignPhase
                    {
                        Phase = phase.GetProperty("phase").GetString() ?? string.Empty,
                        Duration = phase.GetProperty("duration").GetString() ?? string.Empty
                    };

                    if (phase.TryGetProperty("activities", out var activities))
                    {
                        campaignPhase.Activities = activities.EnumerateArray()
                            .Select(a => a.GetString() ?? string.Empty)
                            .Where(s => !string.IsNullOrEmpty(s))
                            .ToList();
                    }

                    strategy.Timeline.Phases.Add(campaignPhase);
                }
            }

            if (arguments.TryGetProperty("successMetrics", out var metrics))
            {
                strategy.SuccessMetrics = metrics.EnumerateArray()
                    .Select(m => m.GetString() ?? string.Empty)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
            }

            return strategy;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing campaign strategy");
            return new CampaignStrategy { BrandName = brandName, CampaignObjective = objective, CampaignTheme = "Error parsing strategy." };
        }
    }

    private TargetAudienceAnalysis ParseTargetAudienceFromFunctionCall(string argumentsJson)
    {
        try
        {
            var arguments = JsonSerializer.Deserialize<JsonElement>(argumentsJson);
            var analysis = new TargetAudienceAnalysis
            {
                PrimaryAudience = new AudienceProfile()
            };

            if (arguments.TryGetProperty("primaryAudience", out var primary))
            {
                analysis.PrimaryAudience.Demographics = primary.GetProperty("demographics").GetString() ?? string.Empty;
                analysis.PrimaryAudience.Psychographics = primary.GetProperty("psychographics").GetString() ?? string.Empty;
                analysis.PrimaryAudience.MediaConsumption = primary.TryGetProperty("mediaConsumption", out var media) ? media.GetString() : null;

                if (primary.TryGetProperty("painPoints", out var painPoints))
                {
                    analysis.PrimaryAudience.PainPoints = painPoints.EnumerateArray()
                        .Select(p => p.GetString() ?? string.Empty)
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToList();
                }

                if (primary.TryGetProperty("motivations", out var motivations))
                {
                    analysis.PrimaryAudience.Motivations = motivations.EnumerateArray()
                        .Select(m => m.GetString() ?? string.Empty)
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToList();
                }
            }

            if (arguments.TryGetProperty("secondaryAudiences", out var secondary))
            {
                analysis.SecondaryAudiences = secondary.EnumerateArray()
                    .Select(s => s.GetString() ?? string.Empty)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
            }

            if (arguments.TryGetProperty("messagingRecommendations", out var messaging))
            {
                analysis.MessagingRecommendations = messaging.EnumerateArray()
                    .Select(m => m.GetString() ?? string.Empty)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
            }

            if (arguments.TryGetProperty("channelRecommendations", out var channels))
            {
                analysis.ChannelRecommendations = channels.EnumerateArray()
                    .Select(c => c.GetString() ?? string.Empty)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
            }

            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing target audience");
            return new TargetAudienceAnalysis { PrimaryAudience = new AudienceProfile(), MessagingRecommendations = new List<string>() };
        }
    }

    private BrandVoice ParseBrandVoiceFromFunctionCall(string argumentsJson, string brandName)
    {
        try
        {
            var arguments = JsonSerializer.Deserialize<JsonElement>(argumentsJson);
            var brandVoice = new BrandVoice
            {
                BrandName = brandName,
                VoiceDescription = arguments.GetProperty("voiceDescription").GetString() ?? string.Empty
            };

            if (arguments.TryGetProperty("toneGuidelines", out var tones))
            {
                foreach (var tone in tones.EnumerateArray())
                {
                    var guideline = new ToneGuideline
                    {
                        Tone = tone.GetProperty("tone").GetString() ?? string.Empty,
                        WhenToUse = tone.GetProperty("whenToUse").GetString() ?? string.Empty
                    };

                    if (tone.TryGetProperty("examples", out var examples))
                    {
                        guideline.Examples = examples.EnumerateArray()
                            .Select(e => e.GetString() ?? string.Empty)
                            .Where(s => !string.IsNullOrEmpty(s))
                            .ToList();
                    }

                    brandVoice.ToneGuidelines.Add(guideline);
                }
            }

            if (arguments.TryGetProperty("doAndDonts", out var doAndDonts))
            {
                brandVoice.DoAndDonts = new DoAndDonts();
                if (doAndDonts.TryGetProperty("do", out var doList))
                {
                    brandVoice.DoAndDonts.Do = doList.EnumerateArray()
                        .Select(d => d.GetString() ?? string.Empty)
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToList();
                }
                if (doAndDonts.TryGetProperty("dont", out var dontList))
                {
                    brandVoice.DoAndDonts.Dont = dontList.EnumerateArray()
                        .Select(d => d.GetString() ?? string.Empty)
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToList();
                }
            }

            if (arguments.TryGetProperty("wordChoices", out var words))
            {
                brandVoice.WordChoices = new WordChoices();
                if (words.TryGetProperty("preferred", out var preferred))
                {
                    brandVoice.WordChoices.Preferred = preferred.EnumerateArray()
                        .Select(p => p.GetString() ?? string.Empty)
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToList();
                }
                if (words.TryGetProperty("avoid", out var avoid))
                {
                    brandVoice.WordChoices.Avoid = avoid.EnumerateArray()
                        .Select(a => a.GetString() ?? string.Empty)
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToList();
                }
            }

            return brandVoice;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing brand voice");
            return new BrandVoice { BrandName = brandName, VoiceDescription = "Error parsing brand voice." };
        }
    }

    private CreativeBrief ParseCreativeBriefFromFunctionCall(string argumentsJson, string clientName)
    {
        try
        {
            var arguments = JsonSerializer.Deserialize<JsonElement>(argumentsJson);
            var brief = new CreativeBrief
            {
                ClientName = clientName,
                Background = arguments.GetProperty("background").GetString() ?? string.Empty,
                Objective = arguments.GetProperty("objective").GetString() ?? string.Empty,
                TargetAudience = arguments.GetProperty("targetAudience").GetString() ?? string.Empty,
                KeyMessage = arguments.GetProperty("keyMessage").GetString() ?? string.Empty
            };

            if (arguments.TryGetProperty("toneOfVoice", out var tone))
                brief.ToneOfVoice = tone.GetString();

            if (arguments.TryGetProperty("deliverables", out var deliverables))
            {
                brief.Deliverables = deliverables.EnumerateArray()
                    .Select(d => d.GetString() ?? string.Empty)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
            }

            if (arguments.TryGetProperty("constraints", out var constraints))
            {
                brief.Constraints = constraints.EnumerateArray()
                    .Select(c => c.GetString() ?? string.Empty)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
            }

            if (arguments.TryGetProperty("successCriteria", out var criteria))
            {
                brief.SuccessCriteria = criteria.EnumerateArray()
                    .Select(c => c.GetString() ?? string.Empty)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
            }

            if (arguments.TryGetProperty("timeline", out var timeline))
                brief.Timeline = timeline.GetString();

            return brief;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing creative brief");
            return new CreativeBrief { ClientName = clientName, Background = "Error parsing brief." };
        }
    }

    private List<ABTestHypothesis> ParseABTestHypothesesFromFunctionCall(string argumentsJson)
    {
        try
        {
            var arguments = JsonSerializer.Deserialize<JsonElement>(argumentsJson);
            var hypotheses = new List<ABTestHypothesis>();

            if (arguments.TryGetProperty("hypotheses", out var hypothesesArray))
            {
                foreach (var hypothesis in hypothesesArray.EnumerateArray())
                {
                    var abHypothesis = new ABTestHypothesis
                    {
                        Hypothesis = hypothesis.GetProperty("hypothesis").GetString() ?? string.Empty,
                        VariantA = hypothesis.GetProperty("variantA").GetString() ?? string.Empty,
                        VariantB = hypothesis.GetProperty("variantB").GetString() ?? string.Empty,
                        ExpectedOutcome = hypothesis.GetProperty("expectedOutcome").GetString() ?? string.Empty
                    };

                    if (hypothesis.TryGetProperty("rationale", out var rationale))
                        abHypothesis.Rationale = rationale.GetString();

                    hypotheses.Add(abHypothesis);
                }
            }

            return hypotheses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing A/B test hypotheses");
            return new List<ABTestHypothesis>();
        }
    }
}

// Data models
public class AdCopy
{
    public string ProductName { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string Headline { get; set; } = string.Empty;
    public string? Subheadline { get; set; }
    public string BodyCopy { get; set; } = string.Empty;
    public string CallToAction { get; set; } = string.Empty;
    public string? Tagline { get; set; }
    public SocialMediaContent? SocialMediaPosts { get; set; }
    public List<string> KeyMessages { get; set; } = new();
}

public class SocialMediaContent
{
    public string? Twitter { get; set; }
    public string? Facebook { get; set; }
    public string? Instagram { get; set; }
    public string? LinkedIn { get; set; }
}

public class CampaignStrategy
{
    public string BrandName { get; set; } = string.Empty;
    public string CampaignObjective { get; set; } = string.Empty;
    public string CampaignTheme { get; set; } = string.Empty;
    public List<string> KeyMessages { get; set; } = new();
    public List<ChannelStrategy> Channels { get; set; } = new();
    public List<AudienceSegment> TargetAudienceSegments { get; set; } = new();
    public CampaignTimeline Timeline { get; set; } = new();
    public List<string> SuccessMetrics { get; set; } = new();
}

public class ChannelStrategy
{
    public string Channel { get; set; } = string.Empty;
    public string Rationale { get; set; } = string.Empty;
    public string? BudgetAllocation { get; set; }
}

public class AudienceSegment
{
    public string Segment { get; set; } = string.Empty;
    public string Characteristics { get; set; } = string.Empty;
    public string Messaging { get; set; } = string.Empty;
}

public class CampaignTimeline
{
    public List<CampaignPhase> Phases { get; set; } = new();
}

public class CampaignPhase
{
    public string Phase { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public List<string> Activities { get; set; } = new();
}

public class TargetAudienceAnalysis
{
    public AudienceProfile PrimaryAudience { get; set; } = new();
    public List<string> SecondaryAudiences { get; set; } = new();
    public List<string> MessagingRecommendations { get; set; } = new();
    public List<string> ChannelRecommendations { get; set; } = new();
}

public class AudienceProfile
{
    public string Demographics { get; set; } = string.Empty;
    public string Psychographics { get; set; } = string.Empty;
    public List<string> PainPoints { get; set; } = new();
    public List<string> Motivations { get; set; } = new();
    public string? MediaConsumption { get; set; }
}

public class BrandVoice
{
    public string BrandName { get; set; } = string.Empty;
    public string VoiceDescription { get; set; } = string.Empty;
    public List<ToneGuideline> ToneGuidelines { get; set; } = new();
    public DoAndDonts? DoAndDonts { get; set; }
    public WordChoices? WordChoices { get; set; }
}

public class ToneGuideline
{
    public string Tone { get; set; } = string.Empty;
    public string WhenToUse { get; set; } = string.Empty;
    public List<string> Examples { get; set; } = new();
}

public class DoAndDonts
{
    public List<string> Do { get; set; } = new();
    public List<string> Dont { get; set; } = new();
}

public class WordChoices
{
    public List<string> Preferred { get; set; } = new();
    public List<string> Avoid { get; set; } = new();
}

public class CreativeBrief
{
    public string ClientName { get; set; } = string.Empty;
    public string Background { get; set; } = string.Empty;
    public string Objective { get; set; } = string.Empty;
    public string TargetAudience { get; set; } = string.Empty;
    public string KeyMessage { get; set; } = string.Empty;
    public string? ToneOfVoice { get; set; }
    public List<string> Deliverables { get; set; } = new();
    public List<string> Constraints { get; set; } = new();
    public List<string> SuccessCriteria { get; set; } = new();
    public string? Timeline { get; set; }
}

public class ABTestHypothesis
{
    public string Hypothesis { get; set; } = string.Empty;
    public string VariantA { get; set; } = string.Empty;
    public string VariantB { get; set; } = string.Empty;
    public string ExpectedOutcome { get; set; } = string.Empty;
    public string? Rationale { get; set; }
}
