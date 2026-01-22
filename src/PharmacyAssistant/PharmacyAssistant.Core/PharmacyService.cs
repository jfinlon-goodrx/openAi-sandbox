using System.Text.Json;
using Microsoft.Extensions.Logging;
using OpenAIShared;

namespace PharmacyAssistant.Core;

/// <summary>
/// Service for pharmacy-related AI tasks: drug information, patient education, interactions, and compliance
/// </summary>
public class PharmacyService
{
    private readonly OpenAIClient _openAIClient;
    private readonly ILogger<PharmacyService> _logger;
    private readonly string _model;

    public PharmacyService(
        OpenAIClient openAIClient,
        ILogger<PharmacyService> logger,
        string model = "gpt-4-turbo-preview")
    {
        _openAIClient = openAIClient;
        _logger = logger;
        _model = model;
    }

    /// <summary>
    /// Generates patient education materials for medications
    /// </summary>
    public async Task<PatientEducation> GeneratePatientEducationAsync(
        string medicationName,
        string? condition = null,
        CancellationToken cancellationToken = default)
    {
        var functionDefinition = new FunctionDefinition
        {
            Name = "generate_patient_education",
            Description = "Generates comprehensive patient education materials",
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    medicationName = new { type = "string" },
                    whatItIs = new { type = "string", description = "What the medication is and what it treats" },
                    howToTake = new { type = "string", description = "How to take the medication" },
                    importantWarnings = new
                    {
                        type = "array",
                        items = new { type = "string" },
                        description = "Important warnings and precautions"
                    },
                    sideEffects = new
                    {
                        type = "array",
                        items = new { type = "string" },
                        description = "Common and serious side effects"
                    },
                    drugInteractions = new
                    {
                        type = "array",
                        items = new { type = "string" },
                        description = "Important drug interactions"
                    },
                    storageInstructions = new { type = "string" },
                    whatToDoIfMissed = new { type = "string" },
                    whenToCallDoctor = new
                    {
                        type = "array",
                        items = new { type = "string" },
                        description = "When to contact healthcare provider"
                    }
                },
                required = new[] { "medicationName", "whatItIs", "howToTake", "importantWarnings" }
            }
        };

        var conditionContext = !string.IsNullOrEmpty(condition) ? $"for {condition}. " : "";
        var prompt = new PromptBuilder()
            .WithSystemMessage("You are a licensed pharmacist with expertise in patient education and medication safety.")
            .WithInstruction($"Generate comprehensive patient education materials for {medicationName} {conditionContext}" +
                           "Include what the medication is, how to take it, warnings, side effects, interactions, storage, " +
                           "what to do if a dose is missed, and when to contact a doctor. Use clear, simple language.")
            .WithInput($"Medication: {medicationName}")
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = "You are a licensed pharmacist specializing in patient education." },
                new() { Role = "user", Content = prompt }
            },
            Functions = new List<FunctionDefinition> { functionDefinition },
            FunctionCall = "auto",
            Temperature = 0.3,
            MaxTokens = 3000
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        var message = response.Choices.FirstOrDefault()?.Message;

        if (message?.FunctionCall != null)
        {
            return ParsePatientEducationFromFunctionCall(message.FunctionCall.Arguments, medicationName);
        }

        return new PatientEducation
        {
            MedicationName = medicationName,
            WhatItIs = message?.Content ?? "Unable to generate patient education."
        };
    }

    /// <summary>
    /// Checks for potential drug interactions
    /// </summary>
    public async Task<DrugInteractionAnalysis> CheckDrugInteractionsAsync(
        List<string> medications,
        List<string>? supplements = null,
        List<string>? conditions = null,
        CancellationToken cancellationToken = default)
    {
        var medsList = string.Join(", ", medications);
        var supplementsList = supplements != null && supplements.Any() ? $"Supplements: {string.Join(", ", supplements)}. " : "";
        var conditionsList = conditions != null && conditions.Any() ? $"Medical conditions: {string.Join(", ", conditions)}. " : "";

        var functionDefinition = new FunctionDefinition
        {
            Name = "analyze_drug_interactions",
            Description = "Analyzes potential drug interactions",
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    interactions = new
                    {
                        type = "array",
                        items = new
                        {
                            type = "object",
                            properties = new
                            {
                                medication1 = new { type = "string" },
                                medication2 = new { type = "string" },
                                severity = new { type = "string", description = "mild, moderate, or severe" },
                                description = new { type = "string" },
                                recommendation = new { type = "string" }
                            },
                            required = new[] { "medication1", "medication2", "severity", "description" }
                        }
                    },
                    overallRisk = new { type = "string", description = "low, moderate, or high" },
                    recommendations = new
                    {
                        type = "array",
                        items = new { type = "string" }
                    },
                    requiresMonitoring = new { type = "boolean" }
                },
                required = new[] { "interactions", "overallRisk" }
            }
        };

        var prompt = new PromptBuilder()
            .WithSystemMessage("You are a clinical pharmacist specializing in drug interaction analysis.")
            .WithInstruction($"Analyze potential drug interactions for the following medications: {medsList}. " +
                           $"{supplementsList}{conditionsList}" +
                           "Identify all potential interactions, their severity, and provide recommendations.")
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = "You are a clinical pharmacist specializing in drug interactions." },
                new() { Role = "user", Content = prompt }
            },
            Functions = new List<FunctionDefinition> { functionDefinition },
            FunctionCall = "auto",
            Temperature = 0.2,
            MaxTokens = 3000
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        var message = response.Choices.FirstOrDefault()?.Message;

        if (message?.FunctionCall != null)
        {
            return ParseDrugInteractionsFromFunctionCall(message.FunctionCall.Arguments);
        }

        return new DrugInteractionAnalysis
        {
            OverallRisk = "Unknown",
            Interactions = new List<DrugInteraction>()
        };
    }

    /// <summary>
    /// Generates prescription label information
    /// </summary>
    public async Task<PrescriptionLabel> GeneratePrescriptionLabelAsync(
        string medicationName,
        string dosage,
        string frequency,
        int quantity,
        string? instructions = null,
        CancellationToken cancellationToken = default)
    {
        var functionDefinition = new FunctionDefinition
        {
            Name = "generate_prescription_label",
            Description = "Generates prescription label information",
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    sig = new { type = "string", description = "Prescription sig (instructions)" },
                    patientInstructions = new { type = "string", description = "Clear patient instructions" },
                    warnings = new
                    {
                        type = "array",
                        items = new { type = "string" }
                    },
                    refills = new { type = "number" },
                    expirationDate = new { type = "string", description = "Days until expiration" }
                },
                required = new[] { "sig", "patientInstructions" }
            }
        };

        var instructionsContext = !string.IsNullOrEmpty(instructions) ? $"Additional instructions: {instructions}. " : "";
        var prompt = new PromptBuilder()
            .WithSystemMessage("You are a pharmacist creating prescription labels.")
            .WithInstruction($"Generate prescription label information for {medicationName}. " +
                           $"Dosage: {dosage}. Frequency: {frequency}. Quantity: {quantity}. " +
                           $"{instructionsContext}" +
                           "Create clear sig, patient instructions, and include appropriate warnings.")
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = "You are a pharmacist creating prescription labels." },
                new() { Role = "user", Content = prompt }
            },
            Functions = new List<FunctionDefinition> { functionDefinition },
            FunctionCall = "auto",
            Temperature = 0.2,
            MaxTokens = 1500
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        var message = response.Choices.FirstOrDefault()?.Message;

        if (message?.FunctionCall != null)
        {
            return ParsePrescriptionLabelFromFunctionCall(message.FunctionCall.Arguments, medicationName, dosage, frequency, quantity);
        }

        return new PrescriptionLabel
        {
            MedicationName = medicationName,
            Dosage = dosage,
            Frequency = frequency,
            Quantity = quantity,
            Sig = "Take as directed",
            PatientInstructions = message?.Content ?? "Unable to generate label."
        };
    }

    /// <summary>
    /// Generates medication adherence reminders and schedules
    /// </summary>
    public async Task<AdherencePlan> GenerateAdherencePlanAsync(
        List<MedicationSchedule> medications,
        CancellationToken cancellationToken = default)
    {
        var medsText = string.Join("\n", medications.Select(m => 
            $"{m.Name}: {m.Dosage} - {m.Frequency} - {m.Timing}"));

        var functionDefinition = new FunctionDefinition
        {
            Name = "generate_adherence_plan",
            Description = "Generates medication adherence plan",
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    dailySchedule = new
                    {
                        type = "array",
                        items = new
                        {
                            type = "object",
                            properties = new
                            {
                                time = new { type = "string" },
                                medications = new
                                {
                                    type = "array",
                                    items = new { type = "string" }
                                },
                                instructions = new { type = "string" }
                            }
                        }
                    },
                    reminders = new
                    {
                        type = "array",
                        items = new { type = "string" }
                    },
                    tips = new
                    {
                        type = "array",
                        items = new { type = "string" }
                    }
                },
                required = new[] { "dailySchedule", "reminders" }
            }
        };

        var prompt = new PromptBuilder()
            .WithSystemMessage("You are a pharmacist helping patients manage multiple medications.")
            .WithInstruction($"Create a medication adherence plan for the following medications:\n{medsText}\n\n" +
                           "Generate a daily schedule, reminders, and tips for better adherence.")
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = "You are a pharmacist helping with medication adherence." },
                new() { Role = "user", Content = prompt }
            },
            Functions = new List<FunctionDefinition> { functionDefinition },
            FunctionCall = "auto",
            Temperature = 0.3,
            MaxTokens = 2000
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        var message = response.Choices.FirstOrDefault()?.Message;

        if (message?.FunctionCall != null)
        {
            return ParseAdherencePlanFromFunctionCall(message.FunctionCall.Arguments);
        }

        return new AdherencePlan
        {
            DailySchedule = new List<ScheduleItem>(),
            Reminders = new List<string>()
        };
    }

    /// <summary>
    /// Analyzes side effects and provides guidance
    /// </summary>
    public async Task<SideEffectAnalysis> AnalyzeSideEffectsAsync(
        string medicationName,
        List<string> reportedSideEffects,
        CancellationToken cancellationToken = default)
    {
        var effectsList = string.Join(", ", reportedSideEffects);

        var functionDefinition = new FunctionDefinition
        {
            Name = "analyze_side_effects",
            Description = "Analyzes medication side effects",
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    commonSideEffects = new
                    {
                        type = "array",
                        items = new
                        {
                            type = "object",
                            properties = new
                            {
                                effect = new { type = "string" },
                                severity = new { type = "string", description = "mild, moderate, or severe" },
                                action = new { type = "string", description = "What to do" }
                            }
                        }
                    },
                    seriousSideEffects = new
                    {
                        type = "array",
                        items = new { type = "string" }
                    },
                    whenToSeekHelp = new
                    {
                        type = "array",
                        items = new { type = "string" }
                    },
                    recommendations = new { type = "string" }
                },
                required = new[] { "commonSideEffects", "whenToSeekHelp" }
            }
        };

        var prompt = new PromptBuilder()
            .WithSystemMessage("You are a clinical pharmacist analyzing medication side effects.")
            .WithInstruction($"Patient reports the following side effects for {medicationName}: {effectsList}. " +
                           "Analyze which are common vs serious, when to seek help, and provide recommendations.")
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = "You are a clinical pharmacist analyzing side effects." },
                new() { Role = "user", Content = prompt }
            },
            Functions = new List<FunctionDefinition> { functionDefinition },
            FunctionCall = "auto",
            Temperature = 0.2,
            MaxTokens = 2000
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        var message = response.Choices.FirstOrDefault()?.Message;

        if (message?.FunctionCall != null)
        {
            return ParseSideEffectsFromFunctionCall(message.FunctionCall.Arguments);
        }

        return new SideEffectAnalysis
        {
            CommonSideEffects = new List<SideEffect>(),
            WhenToSeekHelp = new List<string>()
        };
    }

    // Parsing methods
    private PatientEducation ParsePatientEducationFromFunctionCall(string argumentsJson, string medicationName)
    {
        try
        {
            var arguments = JsonSerializer.Deserialize<JsonElement>(argumentsJson);
            var education = new PatientEducation
            {
                MedicationName = medicationName,
                WhatItIs = arguments.GetProperty("whatItIs").GetString() ?? string.Empty,
                HowToTake = arguments.GetProperty("howToTake").GetString() ?? string.Empty,
                StorageInstructions = arguments.TryGetProperty("storageInstructions", out var storage) ? storage.GetString() : null
            };

            if (arguments.TryGetProperty("importantWarnings", out var warnings))
            {
                education.ImportantWarnings = warnings.EnumerateArray()
                    .Select(w => w.GetString() ?? string.Empty)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
            }

            if (arguments.TryGetProperty("sideEffects", out var effects))
            {
                education.SideEffects = effects.EnumerateArray()
                    .Select(e => e.GetString() ?? string.Empty)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
            }

            if (arguments.TryGetProperty("drugInteractions", out var interactions))
            {
                education.DrugInteractions = interactions.EnumerateArray()
                    .Select(i => i.GetString() ?? string.Empty)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
            }

            if (arguments.TryGetProperty("whatToDoIfMissed", out var missed))
                education.WhatToDoIfMissed = missed.GetString();

            if (arguments.TryGetProperty("whenToCallDoctor", out var call))
            {
                education.WhenToCallDoctor = call.EnumerateArray()
                    .Select(c => c.GetString() ?? string.Empty)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
            }

            return education;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing patient education");
            return new PatientEducation { MedicationName = medicationName, WhatItIs = "Error parsing education." };
        }
    }

    private DrugInteractionAnalysis ParseDrugInteractionsFromFunctionCall(string argumentsJson)
    {
        try
        {
            var arguments = JsonSerializer.Deserialize<JsonElement>(argumentsJson);
            var analysis = new DrugInteractionAnalysis
            {
                OverallRisk = arguments.GetProperty("overallRisk").GetString() ?? "Unknown",
                Interactions = new List<DrugInteraction>()
            };

            if (arguments.TryGetProperty("interactions", out var interactions))
            {
                foreach (var interaction in interactions.EnumerateArray())
                {
                    var drugInteraction = new DrugInteraction
                    {
                        Medication1 = interaction.GetProperty("medication1").GetString() ?? string.Empty,
                        Medication2 = interaction.GetProperty("medication2").GetString() ?? string.Empty,
                        Severity = interaction.GetProperty("severity").GetString() ?? "Unknown",
                        Description = interaction.GetProperty("description").GetString() ?? string.Empty
                    };

                    if (interaction.TryGetProperty("recommendation", out var rec))
                        drugInteraction.Recommendation = rec.GetString();

                    analysis.Interactions.Add(drugInteraction);
                }
            }

            if (arguments.TryGetProperty("recommendations", out var recommendations))
            {
                analysis.Recommendations = recommendations.EnumerateArray()
                    .Select(r => r.GetString() ?? string.Empty)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
            }

            if (arguments.TryGetProperty("requiresMonitoring", out var monitoring))
                analysis.RequiresMonitoring = monitoring.GetBoolean();

            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing drug interactions");
            return new DrugInteractionAnalysis { OverallRisk = "Unknown", Interactions = new List<DrugInteraction>() };
        }
    }

    private PrescriptionLabel ParsePrescriptionLabelFromFunctionCall(string argumentsJson, string medicationName, string dosage, string frequency, int quantity)
    {
        try
        {
            var arguments = JsonSerializer.Deserialize<JsonElement>(argumentsJson);
            var label = new PrescriptionLabel
            {
                MedicationName = medicationName,
                Dosage = dosage,
                Frequency = frequency,
                Quantity = quantity,
                Sig = arguments.GetProperty("sig").GetString() ?? string.Empty,
                PatientInstructions = arguments.GetProperty("patientInstructions").GetString() ?? string.Empty
            };

            if (arguments.TryGetProperty("warnings", out var warnings))
            {
                label.Warnings = warnings.EnumerateArray()
                    .Select(w => w.GetString() ?? string.Empty)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
            }

            if (arguments.TryGetProperty("refills", out var refills))
                label.Refills = refills.GetInt32();

            if (arguments.TryGetProperty("expirationDate", out var expiration))
                label.ExpirationDays = expiration.GetString();

            return label;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing prescription label");
            return new PrescriptionLabel
            {
                MedicationName = medicationName,
                Dosage = dosage,
                Frequency = frequency,
                Quantity = quantity,
                Sig = "Take as directed",
                PatientInstructions = "Error parsing label."
            };
        }
    }

    private AdherencePlan ParseAdherencePlanFromFunctionCall(string argumentsJson)
    {
        try
        {
            var arguments = JsonSerializer.Deserialize<JsonElement>(argumentsJson);
            var plan = new AdherencePlan
            {
                DailySchedule = new List<ScheduleItem>(),
                Reminders = new List<string>()
            };

            if (arguments.TryGetProperty("dailySchedule", out var schedule))
            {
                foreach (var item in schedule.EnumerateArray())
                {
                    var scheduleItem = new ScheduleItem
                    {
                        Time = item.GetProperty("time").GetString() ?? string.Empty,
                        Instructions = item.TryGetProperty("instructions", out var inst) ? inst.GetString() : null
                    };

                    if (item.TryGetProperty("medications", out var meds))
                    {
                        scheduleItem.Medications = meds.EnumerateArray()
                            .Select(m => m.GetString() ?? string.Empty)
                            .Where(s => !string.IsNullOrEmpty(s))
                            .ToList();
                    }

                    plan.DailySchedule.Add(scheduleItem);
                }
            }

            if (arguments.TryGetProperty("reminders", out var reminders))
            {
                plan.Reminders = reminders.EnumerateArray()
                    .Select(r => r.GetString() ?? string.Empty)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
            }

            if (arguments.TryGetProperty("tips", out var tips))
            {
                plan.Tips = tips.EnumerateArray()
                    .Select(t => t.GetString() ?? string.Empty)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
            }

            return plan;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing adherence plan");
            return new AdherencePlan { DailySchedule = new List<ScheduleItem>(), Reminders = new List<string>() };
        }
    }

    private SideEffectAnalysis ParseSideEffectsFromFunctionCall(string argumentsJson)
    {
        try
        {
            var arguments = JsonSerializer.Deserialize<JsonElement>(argumentsJson);
            var analysis = new SideEffectAnalysis
            {
                CommonSideEffects = new List<SideEffect>(),
                WhenToSeekHelp = new List<string>()
            };

            if (arguments.TryGetProperty("commonSideEffects", out var common))
            {
                foreach (var effect in common.EnumerateArray())
                {
                    analysis.CommonSideEffects.Add(new SideEffect
                    {
                        Effect = effect.GetProperty("effect").GetString() ?? string.Empty,
                        Severity = effect.GetProperty("severity").GetString() ?? "Unknown",
                        Action = effect.GetProperty("action").GetString() ?? string.Empty
                    });
                }
            }

            if (arguments.TryGetProperty("seriousSideEffects", out var serious))
            {
                analysis.SeriousSideEffects = serious.EnumerateArray()
                    .Select(s => s.GetString() ?? string.Empty)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
            }

            if (arguments.TryGetProperty("whenToSeekHelp", out var seekHelp))
            {
                analysis.WhenToSeekHelp = seekHelp.EnumerateArray()
                    .Select(s => s.GetString() ?? string.Empty)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
            }

            if (arguments.TryGetProperty("recommendations", out var rec))
                analysis.Recommendations = rec.GetString();

            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing side effects");
            return new SideEffectAnalysis { CommonSideEffects = new List<SideEffect>(), WhenToSeekHelp = new List<string>() };
        }
    }
}

// Data models
public class PatientEducation
{
    public string MedicationName { get; set; } = string.Empty;
    public string WhatItIs { get; set; } = string.Empty;
    public string HowToTake { get; set; } = string.Empty;
    public List<string> ImportantWarnings { get; set; } = new();
    public List<string> SideEffects { get; set; } = new();
    public List<string> DrugInteractions { get; set; } = new();
    public string? StorageInstructions { get; set; }
    public string? WhatToDoIfMissed { get; set; }
    public List<string> WhenToCallDoctor { get; set; } = new();
}

public class DrugInteractionAnalysis
{
    public List<DrugInteraction> Interactions { get; set; } = new();
    public string OverallRisk { get; set; } = string.Empty;
    public List<string> Recommendations { get; set; } = new();
    public bool RequiresMonitoring { get; set; }
}

public class DrugInteraction
{
    public string Medication1 { get; set; } = string.Empty;
    public string Medication2 { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Recommendation { get; set; }
}

public class PrescriptionLabel
{
    public string MedicationName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Sig { get; set; } = string.Empty;
    public string PatientInstructions { get; set; } = string.Empty;
    public List<string> Warnings { get; set; } = new();
    public int? Refills { get; set; }
    public string? ExpirationDays { get; set; }
}

public class MedicationSchedule
{
    public string Name { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public string Timing { get; set; } = string.Empty; // e.g., "with food", "before bed"
}

public class AdherencePlan
{
    public List<ScheduleItem> DailySchedule { get; set; } = new();
    public List<string> Reminders { get; set; } = new();
    public List<string> Tips { get; set; } = new();
}

public class ScheduleItem
{
    public string Time { get; set; } = string.Empty;
    public List<string> Medications { get; set; } = new();
    public string? Instructions { get; set; }
}

public class SideEffectAnalysis
{
    public List<SideEffect> CommonSideEffects { get; set; } = new();
    public List<string> SeriousSideEffects { get; set; } = new();
    public List<string> WhenToSeekHelp { get; set; } = new();
    public string? Recommendations { get; set; }
}

public class SideEffect
{
    public string Effect { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
}
