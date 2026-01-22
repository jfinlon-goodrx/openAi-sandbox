# Pharmacy Assistant

## Overview

The Pharmacy Assistant helps pharmacies automate patient education, drug interaction checking, prescription label generation, medication adherence planning, and side effect analysis.

## Features

- **Patient Education Materials**: Generate comprehensive patient education for medications
- **Drug Interaction Checking**: Analyze potential interactions between medications, supplements, and conditions
- **Prescription Label Generation**: Create clear, compliant prescription labels
- **Medication Adherence Planning**: Generate schedules and reminders for multiple medications
- **Side Effect Analysis**: Analyze reported side effects and provide guidance

## Architecture

```
┌─────────────────┐
│   Web API       │
└────────┬────────┘
         │
┌────────▼────────┐
│ Pharmacy Service │
└────────┬────────┘
         │
┌────────▼────────┐
│  OpenAI Client  │
└─────────────────┘
```

## Setup

1. Configure OpenAI API key (see [Setup Guide](../getting-started/01-setup.md))

2. Run the API:
```bash
cd src/PharmacyAssistant/PharmacyAssistant.Api
dotnet run
```

3. Navigate to `https://localhost:7004/swagger` for API documentation

## API Endpoints

### POST /api/pharmacy/patient-education

Generates patient education materials.

**Request:**
```json
{
  "medicationName": "Metformin",
  "condition": "Type 2 Diabetes"
}
```

**Response:**
```json
{
  "medicationName": "Metformin",
  "whatItIs": "Metformin is used to treat type 2 diabetes...",
  "howToTake": "Take with meals to reduce stomach upset...",
  "importantWarnings": ["Do not take if you have kidney problems", "Avoid alcohol"],
  "sideEffects": ["Nausea", "Diarrhea", "Stomach upset"],
  "drugInteractions": ["May interact with certain heart medications"],
  "whenToCallDoctor": ["Severe stomach pain", "Difficulty breathing"]
}
```

### POST /api/pharmacy/check-interactions

Checks for drug interactions.

**Request:**
```json
{
  "medications": ["Metformin", "Lisinopril", "Aspirin"],
  "supplements": ["Vitamin D", "Fish Oil"],
  "conditions": ["Type 2 Diabetes", "Hypertension"]
}
```

**Response:**
```json
{
  "interactions": [
    {
      "medication1": "Metformin",
      "medication2": "Lisinopril",
      "severity": "moderate",
      "description": "May increase risk of lactic acidosis",
      "recommendation": "Monitor kidney function"
    }
  ],
  "overallRisk": "moderate",
  "recommendations": ["Monitor kidney function", "Regular blood tests"],
  "requiresMonitoring": true
}
```

### POST /api/pharmacy/prescription-label

Generates prescription label.

**Request:**
```json
{
  "medicationName": "Metformin",
  "dosage": "500mg",
  "frequency": "Twice daily",
  "quantity": 60,
  "instructions": "Take with food"
}
```

**Response:**
```json
{
  "medicationName": "Metformin",
  "dosage": "500mg",
  "frequency": "Twice daily",
  "quantity": 60,
  "sig": "Take 1 tablet by mouth twice daily with meals",
  "patientInstructions": "Take with breakfast and dinner to reduce stomach upset",
  "warnings": ["Do not skip meals", "Avoid alcohol"],
  "refills": 3
}
```

### POST /api/pharmacy/adherence-plan

Generates medication adherence plan.

**Request:**
```json
{
  "medications": [
    {
      "name": "Metformin",
      "dosage": "500mg",
      "frequency": "Twice daily",
      "timing": "with meals"
    },
    {
      "name": "Lisinopril",
      "dosage": "10mg",
      "frequency": "Once daily",
      "timing": "in the morning"
    }
  ]
}
```

**Response:**
```json
{
  "dailySchedule": [
    {
      "time": "8:00 AM",
      "medications": ["Lisinopril 10mg"],
      "instructions": "Take with breakfast"
    },
    {
      "time": "8:00 PM",
      "medications": ["Metformin 500mg"],
      "instructions": "Take with dinner"
    }
  ],
  "reminders": ["Set phone alarms", "Use pill organizer"],
  "tips": ["Take at same times daily", "Never skip doses"]
}
```

### POST /api/pharmacy/analyze-side-effects

Analyzes side effects.

**Request:**
```json
{
  "medicationName": "Metformin",
  "reportedSideEffects": ["Nausea", "Stomach pain", "Dizziness"]
}
```

**Response:**
```json
{
  "commonSideEffects": [
    {
      "effect": "Nausea",
      "severity": "mild",
      "action": "Take with food, usually resolves in 1-2 weeks"
    }
  ],
  "seriousSideEffects": ["Lactic acidosis", "Severe allergic reaction"],
  "whenToSeekHelp": ["Severe stomach pain", "Difficulty breathing"],
  "recommendations": "Most side effects are mild and temporary"
}
```

## Usage Examples

### Generate Patient Education

```csharp
var pharmacyService = new PharmacyService(openAIClient, logger);

var education = await pharmacyService.GeneratePatientEducationAsync(
    "Metformin",
    condition: "Type 2 Diabetes"
);

Console.WriteLine($"What it is: {education.WhatItIs}");
Console.WriteLine($"How to take: {education.HowToTake}");
Console.WriteLine("\nWarnings:");
foreach (var warning in education.ImportantWarnings)
{
    Console.WriteLine($"- {warning}");
}
```

### Check Drug Interactions

```csharp
var medications = new List<string> { "Metformin", "Lisinopril", "Aspirin" };
var analysis = await pharmacyService.CheckDrugInteractionsAsync(
    medications,
    supplements: new List<string> { "Vitamin D" },
    conditions: new List<string> { "Type 2 Diabetes" }
);

Console.WriteLine($"Overall Risk: {analysis.OverallRisk}");
foreach (var interaction in analysis.Interactions)
{
    Console.WriteLine($"{interaction.Medication1} + {interaction.Medication2}: {interaction.Severity}");
}
```

## Best Practices

1. **Always Verify**: AI-generated information should be verified by licensed pharmacists
2. **Use as Starting Point**: Use AI output as a starting point, then customize for specific patients
3. **Compliance**: Ensure all outputs comply with pharmacy regulations and standards
4. **Patient Safety**: Always prioritize patient safety over convenience
5. **Documentation**: Keep records of AI-generated content for audit purposes

## Important Disclaimers

⚠️ **This tool is for assistance only. All AI-generated information must be reviewed and verified by licensed healthcare professionals before use with patients.**

## Related Documentation

- [Getting Started](../getting-started/)
- [Best Practices](../best-practices/)
