using Microsoft.AspNetCore.Mvc;
using PharmacyAssistant.Core;

namespace PharmacyAssistant.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PharmacyController : ControllerBase
{
    private readonly PharmacyService _pharmacyService;
    private readonly ILogger<PharmacyController> _logger;

    public PharmacyController(
        PharmacyService pharmacyService,
        ILogger<PharmacyController> logger)
    {
        _pharmacyService = pharmacyService;
        _logger = logger;
    }

    /// <summary>
    /// Generates patient education materials
    /// </summary>
    [HttpPost("patient-education")]
    public async Task<ActionResult<PatientEducation>> GeneratePatientEducation([FromBody] PatientEducationRequest request)
    {
        try
        {
            var education = await _pharmacyService.GeneratePatientEducationAsync(
                request.MedicationName,
                request.Condition);
            return Ok(education);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating patient education");
            return StatusCode(500, new { error = "Failed to generate patient education", message = ex.Message });
        }
    }

    /// <summary>
    /// Checks for drug interactions
    /// </summary>
    [HttpPost("check-interactions")]
    public async Task<ActionResult<DrugInteractionAnalysis>> CheckInteractions([FromBody] InteractionCheckRequest request)
    {
        try
        {
            var analysis = await _pharmacyService.CheckDrugInteractionsAsync(
                request.Medications,
                request.Supplements,
                request.Conditions);
            return Ok(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking interactions");
            return StatusCode(500, new { error = "Failed to check interactions", message = ex.Message });
        }
    }

    /// <summary>
    /// Generates prescription label
    /// </summary>
    [HttpPost("prescription-label")]
    public async Task<ActionResult<PrescriptionLabel>> GeneratePrescriptionLabel([FromBody] PrescriptionLabelRequest request)
    {
        try
        {
            var label = await _pharmacyService.GeneratePrescriptionLabelAsync(
                request.MedicationName,
                request.Dosage,
                request.Frequency,
                request.Quantity,
                request.Instructions);
            return Ok(label);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating prescription label");
            return StatusCode(500, new { error = "Failed to generate prescription label", message = ex.Message });
        }
    }

    /// <summary>
    /// Generates medication adherence plan
    /// </summary>
    [HttpPost("adherence-plan")]
    public async Task<ActionResult<AdherencePlan>> GenerateAdherencePlan([FromBody] AdherencePlanRequest request)
    {
        try
        {
            var plan = await _pharmacyService.GenerateAdherencePlanAsync(request.Medications);
            return Ok(plan);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating adherence plan");
            return StatusCode(500, new { error = "Failed to generate adherence plan", message = ex.Message });
        }
    }

    /// <summary>
    /// Analyzes side effects
    /// </summary>
    [HttpPost("analyze-side-effects")]
    public async Task<ActionResult<SideEffectAnalysis>> AnalyzeSideEffects([FromBody] SideEffectRequest request)
    {
        try
        {
            var analysis = await _pharmacyService.AnalyzeSideEffectsAsync(
                request.MedicationName,
                request.ReportedSideEffects);
            return Ok(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing side effects");
            return StatusCode(500, new { error = "Failed to analyze side effects", message = ex.Message });
        }
    }
}

public class PatientEducationRequest
{
    public string MedicationName { get; set; } = string.Empty;
    public string? Condition { get; set; }
}

public class InteractionCheckRequest
{
    public List<string> Medications { get; set; } = new();
    public List<string>? Supplements { get; set; }
    public List<string>? Conditions { get; set; }
}

public class PrescriptionLabelRequest
{
    public string MedicationName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? Instructions { get; set; }
}

public class AdherencePlanRequest
{
    public List<MedicationSchedule> Medications { get; set; } = new();
}

public class SideEffectRequest
{
    public string MedicationName { get; set; } = string.Empty;
    public List<string> ReportedSideEffects { get; set; } = new();
}
