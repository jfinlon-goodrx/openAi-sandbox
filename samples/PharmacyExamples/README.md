# Pharmacy Assistant Examples

Examples for using the Pharmacy Assistant API in pharmacy workflows.

## Examples

### 1. Generate Patient Education

```csharp
var pharmacyService = new PharmacyService(openAIClient, logger);

var education = await pharmacyService.GeneratePatientEducationAsync(
    "Metformin",
    condition: "Type 2 Diabetes"
);

Console.WriteLine($"Medication: {education.MedicationName}");
Console.WriteLine($"\nWhat it is:\n{education.WhatItIs}");
Console.WriteLine($"\nHow to take:\n{education.HowToTake}");
Console.WriteLine("\nImportant Warnings:");
foreach (var warning in education.ImportantWarnings)
{
    Console.WriteLine($"- {warning}");
}
```

### 2. Check Drug Interactions

```csharp
var medications = new List<string> { "Metformin", "Lisinopril", "Aspirin" };
var supplements = new List<string> { "Vitamin D", "Fish Oil" };
var conditions = new List<string> { "Type 2 Diabetes", "Hypertension" };

var analysis = await pharmacyService.CheckDrugInteractionsAsync(
    medications,
    supplements,
    conditions
);

Console.WriteLine($"Overall Risk: {analysis.OverallRisk}");
Console.WriteLine("\nInteractions:");
foreach (var interaction in analysis.Interactions)
{
    Console.WriteLine($"{interaction.Medication1} + {interaction.Medication2}");
    Console.WriteLine($"  Severity: {interaction.Severity}");
    Console.WriteLine($"  Description: {interaction.Description}");
    if (!string.IsNullOrEmpty(interaction.Recommendation))
    {
        Console.WriteLine($"  Recommendation: {interaction.Recommendation}");
    }
}
```

### 3. Generate Prescription Label

```csharp
var label = await pharmacyService.GeneratePrescriptionLabelAsync(
    medicationName: "Metformin",
    dosage: "500mg",
    frequency: "Twice daily",
    quantity: 60,
    instructions: "Take with food"
);

Console.WriteLine($"Medication: {label.MedicationName}");
Console.WriteLine($"Sig: {label.Sig}");
Console.WriteLine($"Patient Instructions: {label.PatientInstructions}");
Console.WriteLine("\nWarnings:");
foreach (var warning in label.Warnings)
{
    Console.WriteLine($"- {warning}");
}
```

### 4. Create Adherence Plan

```csharp
var medications = new List<MedicationSchedule>
{
    new() { Name = "Metformin", Dosage = "500mg", Frequency = "Twice daily", Timing = "with meals" },
    new() { Name = "Lisinopril", Dosage = "10mg", Frequency = "Once daily", Timing = "in the morning" },
    new() { Name = "Aspirin", Dosage = "81mg", Frequency = "Once daily", Timing = "with breakfast" }
};

var plan = await pharmacyService.GenerateAdherencePlanAsync(medications);

Console.WriteLine("Daily Schedule:");
foreach (var item in plan.DailySchedule)
{
    Console.WriteLine($"\n{item.Time}:");
    foreach (var med in item.Medications)
    {
        Console.WriteLine($"  - {med}");
    }
    if (!string.IsNullOrEmpty(item.Instructions))
    {
        Console.WriteLine($"  Instructions: {item.Instructions}");
    }
}

Console.WriteLine("\nReminders:");
foreach (var reminder in plan.Reminders)
{
    Console.WriteLine($"- {reminder}");
}
```

### 5. Analyze Side Effects

```csharp
var reportedSideEffects = new List<string> { "Nausea", "Stomach pain", "Dizziness" };

var analysis = await pharmacyService.AnalyzeSideEffectsAsync(
    "Metformin",
    reportedSideEffects
);

Console.WriteLine("Common Side Effects:");
foreach (var effect in analysis.CommonSideEffects)
{
    Console.WriteLine($"- {effect.Effect} ({effect.Severity}): {effect.Action}");
}

Console.WriteLine("\nWhen to Seek Help:");
foreach (var item in analysis.WhenToSeekHelp)
{
    Console.WriteLine($"- {item}");
}
```

## Complete Workflow Example

```csharp
// 1. Generate patient education
var education = await pharmacyService.GeneratePatientEducationAsync("Metformin", "Type 2 Diabetes");

// 2. Check interactions
var interactions = await pharmacyService.CheckDrugInteractionsAsync(
    new List<string> { "Metformin", "Lisinopril" }
);

// 3. Generate prescription label
var label = await pharmacyService.GeneratePrescriptionLabelAsync(
    "Metformin", "500mg", "Twice daily", 60
);

// 4. Create adherence plan
var plan = await pharmacyService.GenerateAdherencePlanAsync(
    new List<MedicationSchedule> { /* medications */ }
);

// 5. Provide to patient
// Print education materials, label, and adherence plan
```

## API Usage

Start the API:
```bash
cd src/PharmacyAssistant/PharmacyAssistant.Api
dotnet run
```

**Generate Patient Education:**
```bash
curl -X POST https://localhost:7004/api/pharmacy/patient-education \
  -H "Content-Type: application/json" \
  -d '{"medicationName": "Metformin", "condition": "Type 2 Diabetes"}'
```

**Check Interactions:**
```bash
curl -X POST https://localhost:7004/api/pharmacy/check-interactions \
  -H "Content-Type: application/json" \
  -d '{"medications": ["Metformin", "Lisinopril"]}'
```

## Important Notes

⚠️ **All AI-generated information must be reviewed and verified by licensed pharmacists before use with patients.**
