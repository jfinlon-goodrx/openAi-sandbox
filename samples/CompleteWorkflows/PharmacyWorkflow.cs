using PharmacyAssistant.Core;
using OpenAIShared;

namespace Samples.CompleteWorkflows;

/// <summary>
/// Complete pharmacy workflow example demonstrating multiple AI capabilities
/// </summary>
public class PharmacyWorkflow
{
    private readonly PharmacyService _pharmacyService;
    private readonly ModerationService _moderationService;
    private readonly RAGService _ragService;

    public PharmacyWorkflow(
        PharmacyService pharmacyService,
        ModerationService moderationService,
        RAGService ragService)
    {
        _pharmacyService = pharmacyService;
        _moderationService = moderationService;
        _ragService = ragService;
    }

    /// <summary>
    /// Complete workflow: New prescription processing
    /// </summary>
    public async Task<PrescriptionWorkflowResult> ProcessNewPrescriptionAsync(
        string medicationName,
        string dosage,
        string frequency,
        int quantity,
        List<string> existingMedications,
        CancellationToken cancellationToken = default)
    {
        var result = new PrescriptionWorkflowResult();

        // Step 1: Check for interactions
        var allMeds = new List<string>(existingMedications) { medicationName };
        var interactionAnalysis = await _pharmacyService.CheckDrugInteractionsAsync(
            allMeds,
            cancellationToken: cancellationToken);
        result.InteractionAnalysis = interactionAnalysis;

        // Step 2: Generate patient education
        var education = await _pharmacyService.GeneratePatientEducationAsync(
            medicationName,
            cancellationToken: cancellationToken);
        result.PatientEducation = education;

        // Step 3: Generate prescription label
        var label = await _pharmacyService.GeneratePrescriptionLabelAsync(
            medicationName,
            dosage,
            frequency,
            quantity,
            cancellationToken: cancellationToken);
        result.PrescriptionLabel = label;

        // Step 4: Update adherence plan if multiple medications
        if (existingMedications.Any())
        {
            var schedules = existingMedications.Select(m => new MedicationSchedule
            {
                Name = m,
                Dosage = "As prescribed",
                Frequency = "As prescribed",
                Timing = ""
            }).ToList();

            schedules.Add(new MedicationSchedule
            {
                Name = medicationName,
                Dosage = dosage,
                Frequency = frequency,
                Timing = "As directed"
            });

            var adherencePlan = await _pharmacyService.GenerateAdherencePlanAsync(
                schedules,
                cancellationToken: cancellationToken);
            result.AdherencePlan = adherencePlan;
        }

        return result;
    }

    /// <summary>
    /// Complete workflow: Patient question handling with RAG
    /// </summary>
    public async Task<string> AnswerPatientQuestionAsync(
        string question,
        List<Document> knowledgeBase,
        CancellationToken cancellationToken = default)
    {
        // Step 1: Moderate question
        var moderation = await _moderationService.ModerateContentAsync(question, cancellationToken);
        if (moderation.Results.First().Flagged)
        {
            return "I'm unable to answer that question. Please consult with your pharmacist directly.";
        }

        // Step 2: Create embeddings if not already done
        var embeddings = await _ragService.CreateDocumentEmbeddingsAsync(knowledgeBase, cancellationToken);

        // Step 3: Answer using RAG
        var answer = await _ragService.QueryWithRAGAsync(
            question,
            embeddings,
            topK: 3,
            cancellationToken: cancellationToken);

        // Step 4: Moderate answer before returning
        var answerModeration = await _moderationService.ModerateContentAsync(answer, cancellationToken);
        if (answerModeration.Results.First().Flagged)
        {
            return "I'm unable to provide that information. Please consult with your pharmacist.";
        }

        return answer;
    }
}

public class PrescriptionWorkflowResult
{
    public DrugInteractionAnalysis? InteractionAnalysis { get; set; }
    public PatientEducation? PatientEducation { get; set; }
    public PrescriptionLabel? PrescriptionLabel { get; set; }
    public AdherencePlan? AdherencePlan { get; set; }
}
