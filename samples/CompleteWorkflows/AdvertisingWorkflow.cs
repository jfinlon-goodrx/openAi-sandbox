using AdvertisingAgency.Core;
using OpenAIShared;

namespace Samples.CompleteWorkflows;

/// <summary>
/// Complete advertising campaign workflow demonstrating multiple AI capabilities
/// </summary>
public class AdvertisingWorkflow
{
    private readonly AdvertisingService _advertisingService;
    private readonly ModerationService _moderationService;
    private readonly VisionService _visionService;

    public AdvertisingWorkflow(
        AdvertisingService advertisingService,
        ModerationService moderationService,
        VisionService visionService)
    {
        _advertisingService = advertisingService;
        _moderationService = moderationService;
        _visionService = visionService;
    }

    /// <summary>
    /// Complete workflow: New campaign development
    /// </summary>
    public async Task<CampaignWorkflowResult> DevelopCampaignAsync(
        string brandName,
        string productDescription,
        string targetAudience,
        string campaignObjective,
        decimal budget,
        CancellationToken cancellationToken = default)
    {
        var result = new CampaignWorkflowResult();

        // Step 1: Analyze target audience
        var audienceAnalysis = await _advertisingService.AnalyzeTargetAudienceAsync(
            productDescription,
            cancellationToken: cancellationToken);
        result.AudienceAnalysis = audienceAnalysis;

        // Step 2: Develop brand voice (if not exists)
        var brandVoice = await _advertisingService.DevelopBrandVoiceAsync(
            brandName,
            productDescription,
            cancellationToken: cancellationToken);
        result.BrandVoice = brandVoice;

        // Step 3: Develop campaign strategy
        var strategy = await _advertisingService.DevelopCampaignStrategyAsync(
            brandName,
            productDescription,
            targetAudience,
            campaignObjective,
            budget,
            cancellationToken: cancellationToken);
        result.CampaignStrategy = strategy;

        // Step 4: Generate creative brief
        var brief = await _advertisingService.GenerateCreativeBriefAsync(
            brandName,
            productDescription,
            campaignObjective,
            targetAudience,
            budget.ToString("C"),
            cancellationToken: cancellationToken);
        result.CreativeBrief = brief;

        // Step 5: Generate ad copy for each channel
        var adCopies = new List<AdCopy>();
        foreach (var channel in strategy.Channels)
        {
            var adCopy = await _advertisingService.GenerateAdCopyAsync(
                brandName,
                productDescription,
                targetAudience,
                channel.Channel,
                tone: brandVoice.ToneGuidelines.FirstOrDefault()?.Tone,
                cancellationToken: cancellationToken);

            // Moderate ad copy
            if (adCopy.BodyCopy != null)
            {
                var moderation = await _moderationService.ModerateContentAsync(adCopy.BodyCopy, cancellationToken);
                if (!moderation.Results.First().Flagged)
                {
                    adCopies.Add(adCopy);
                }
            }
        }
        result.AdCopies = adCopies;

        // Step 6: Generate A/B test hypotheses
        var hypotheses = await _advertisingService.GenerateABTestHypothesesAsync(
            campaignObjective,
            metric: "Click-through rate",
            cancellationToken: cancellationToken);
        result.ABTestHypotheses = hypotheses;

        return result;
    }

    /// <summary>
    /// Analyze ad creative images
    /// </summary>
    public async Task<string> AnalyzeAdCreativeAsync(
        string imageUrl,
        string campaignObjective,
        CancellationToken cancellationToken = default)
    {
        var prompt = $"Analyze this advertisement creative. " +
                    $"Campaign objective: {campaignObjective}. " +
                    "Evaluate: visual hierarchy, call-to-action visibility, brand presence, " +
                    "target audience appeal, and overall effectiveness. " +
                    "Provide specific recommendations for improvement.";

        return await _visionService.AnalyzeImageAsync(imageUrl, prompt, "high", cancellationToken);
    }
}

public class CampaignWorkflowResult
{
    public TargetAudienceAnalysis? AudienceAnalysis { get; set; }
    public BrandVoice? BrandVoice { get; set; }
    public CampaignStrategy? CampaignStrategy { get; set; }
    public CreativeBrief? CreativeBrief { get; set; }
    public List<AdCopy> AdCopies { get; set; } = new();
    public List<ABTestHypothesis> ABTestHypotheses { get; set; } = new();
}
