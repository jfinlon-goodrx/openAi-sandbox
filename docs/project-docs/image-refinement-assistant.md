# Image Refinement Assistant

**For:** Designers, marketers, content creators, and developers who need to generate high-quality images systematically and select the best candidates.

**What you'll learn:** How to use DALL-E for image generation, GPT-4 Vision for image evaluation, systematic refinement workflows, and automated quality assessment to select the best images from multiple generations.

## Overview

The Image Refinement Assistant automates the process of generating multiple images from prompts, evaluating them systematically using AI vision analysis, and selecting the top candidates for human review. This workflow ensures you get the highest quality results by generating 5-10 variations and letting AI evaluate them before presenting the best options.

**Why use AI for image refinement?**
- **Systematic evaluation:** AI objectively evaluates images based on quality, relevance, composition, and visual appeal
- **Time savings:** Automatically generates and evaluates multiple images, eliminating manual review of every variation
- **Consistent quality:** Standardized evaluation criteria ensure all candidates meet quality thresholds
- **Cost optimization:** Only generate the number of images needed, then intelligently select the best
- **Scalability:** Process multiple prompts simultaneously and evaluate large batches efficiently
- **Objective scoring:** AI provides numerical scores and detailed feedback for each image

## Features

- **Batch Image Generation**: Generate multiple images from one or more prompts using DALL-E 3
- **AI-Powered Evaluation**: Use GPT-4 Vision to evaluate each image for quality, relevance, and visual appeal
- **Automated Ranking**: Images are automatically scored and ranked by overall quality
- **Top Candidate Selection**: Returns the top N images (default: top 3) for human review
- **Visual Web Interface**: ⭐ NEW - Browse all generated images in a beautiful gallery with prompts, scores, and detailed feedback
- **Image Download**: ⭐ NEW - Download any image directly from the gallery with one click. Images are saved with descriptive filenames based on prompts
- **Star Ratings & Scores**: Display scores as both star ratings (★) and numerical scores (x/10)
- **Sorted Display**: All images displayed sorted by score (best to worst) for easy comparison
- **Detailed Feedback**: Each image includes scores, strengths, weaknesses, and evaluation details
- **Custom Evaluation Criteria**: Specify custom evaluation criteria for domain-specific requirements
- **Flexible Configuration**: Control image size, quality, model selection, and generation count

## Architecture

```
┌─────────────────┐     ┌─────────────────┐
│   Blazor Web    │────▶│   Web API       │
│      UI         │     └────────┬────────┘
└─────────────────┘              │
                                 │
                        ┌────────▼────────┐
                        │ Image          │
                        │ Refinement     │
                        │ Service        │
                        └────────┬────────┘
                                 │
                            ┌────┴────┐
                            │         │
                        ┌───▼───┐ ┌──▼──────┐
                        │ DALL-E│ │ Vision  │
                        │ API   │ │ API     │
                        └───────┘ └─────────┘
```

## Workflow

1. **Generate Images**: Create multiple images (5-10) from provided prompts using DALL-E
2. **Evaluate Images**: Use GPT-4 Vision to analyze each image and assign quality/relevance scores
3. **Rank Results**: Sort images by overall score (average of quality and relevance)
4. **Select Top Candidates**: Return the top N images for human selection
5. **Provide Feedback**: Include detailed evaluation with strengths, weaknesses, and scores

## Configuration

### Storage Path

By default, exports are saved to: `~/Downloads/ImageRefinementAssistant/`

To customize the storage location, add to `appsettings.json`:

```json
{
  "ImageRefinement": {
    "StoragePath": "/path/to/your/custom/storage"
  }
}
```

If not specified, defaults to `~/Downloads/ImageRefinementAssistant/`

## Quick Start

### Option 1: Docker (Recommended - Easy Setup)

**Prerequisites:**
- Docker Desktop installed
- `.env` file in project root with `OPENAI_API_KEY`

**Steps:**

1. **Load environment variables:**
   ```bash
   source scripts/setup-env.sh
   ```

2. **Start services with Docker Compose:**
   ```bash
   docker-compose up -d image-refinement-api image-refinement-web
   ```

3. **View logs:**
   ```bash
   docker-compose logs -f image-refinement-api image-refinement-web
   ```

4. **Access the applications:**
   - **Web UI**: http://localhost:5000
   - **API**: http://localhost:5001
   - **API Swagger**: http://localhost:5001/swagger

5. **Stop services:**
   ```bash
   docker-compose stop image-refinement-api image-refinement-web
   ```

**Debugging in Docker:**
- Services run with `dotnet watch` for hot reload
- VS Code debugging: Use "Attach to Image Refinement API (Docker)" or "Attach to Image Refinement Web (Docker)" configurations
- Debug ports: API (5002), Web (5003)

### Option 2: Local Development

**Prerequisites:**
- .NET 8.0 SDK installed
- `.env` file in project root with `OPENAI_API_KEY`

**Steps:**

1. **Load your API key** from `.env` file:
   ```bash
   # macOS/Linux
   source scripts/setup-env.sh
   
   # Windows PowerShell
   .\scripts\setup-env.ps1
   
   # Windows Command Prompt
   scripts\setup-env.bat
   ```

   See [README-SECURITY.md](../../README-SECURITY.md) for complete setup instructions.

### Running the Project

**Option 1: Web UI (Recommended - Visual Gallery)**

1. **Start the API** (Terminal 1):
   ```bash
   cd src/ImageRefinementAssistant/ImageRefinementAssistant.Api
   dotnet run
   ```
   
   The API will start on `http://localhost:5001` (or check console output for actual port).

2. **Start the Web UI** (Terminal 2):
   ```bash
   cd src/ImageRefinementAssistant/ImageRefinementAssistant.Web
   dotnet run
   ```
   
   The Web UI will start on `https://localhost:5002` (or check console output for actual port).

3. **Open your browser** to the Web UI URL shown in the console (typically `https://localhost:5002`)

4. **Use the visual gallery** to:
   - Enter prompts (one per line)
   - Configure number of images to generate
   - Set top images to return
   - Click "Generate & Evaluate Images"
   - Browse the gallery sorted by score with prompts and detailed feedback
   - **Download any image** by clicking the "💾 Save Image" button (important: DALL-E URLs expire, so download promptly!)

**Option 2: API Only (REST API)**

1. **Start the API**:
   ```bash
   cd src/ImageRefinementAssistant/ImageRefinementAssistant.Api
   dotnet run
   ```

2. **Access Swagger UI** at `http://localhost:5001/swagger` (or check console for actual port)

3. **Use curl, Python, or Postman** to call the API (see API Usage Examples below)

### Verifying Setup

**Check API is running:**
```bash
curl http://localhost:5001/api/imagerefinement/health
```

**Expected response:**
```json
{
  "status": "healthy",
  "service": "ImageRefinementAssistant"
}
```

## API Endpoints

### POST /api/imagerefinement/refine

Generates multiple images, evaluates them, and returns the top candidates.

**Request:**
```json
{
  "prompts": [
    "A futuristic cityscape at sunset with flying cars and neon lights",
    "A serene mountain landscape with a crystal-clear lake reflecting snow-capped peaks"
  ],
  "numberOfImagesToGenerate": 8,
  "topImagesToReturn": 3,
  "evaluationCriteria": "Focus on visual appeal, composition quality, and color harmony",
  "size": "1024x1024",
  "quality": "standard",
  "model": "dall-e-3"
}
```

**Request Parameters:**
- `prompts` (required): List of prompts to generate images from
- `numberOfImagesToGenerate` (optional, default: 8): Total number of images to generate
- `topImagesToReturn` (optional, default: 3): Number of top images to return
- `evaluationCriteria` (optional): Custom criteria for evaluation (e.g., "Focus on professional design")
- `size` (optional, default: "1024x1024"): Image size - "1024x1024", "1792x1024", or "1024x1792"
- `quality` (optional, default: "standard"): Image quality - "standard" or "hd"
- `model` (optional, default: "dall-e-3"): Model to use - "dall-e-3" or "dall-e-2"

**Response:**
```json
{
  "originalDescription": "A futuristic cityscape...; A serene mountain...",
  "totalImagesGenerated": 8,
  "imagesEvaluated": 8,
  "summary": "Generated 8 images from 2 prompt(s). Evaluated 8 images and selected the top 3 candidates...",
  "processedAt": "2026-01-23T10:30:00Z",
  "allGeneratedImages": [
    {
      "imageUrl": "https://oaidalleapiprodscus.blob.core.windows.net/...",
      "prompt": "A futuristic cityscape at sunset...",
      "revisedPrompt": "A futuristic cityscape at sunset...",
      "qualityScore": 8.5,
      "relevanceScore": 9.0,
      "overallScore": 8.75,
      "evaluationDetails": "This image demonstrates excellent technical quality...",
      "strengths": [
        "Strong color harmony",
        "Excellent composition",
        "High detail level"
      ],
      "weaknesses": [
        "Slight overexposure in sky area"
      ],
      "generatedAt": "2026-01-23T10:29:45Z",
      "evaluatedAt": "2026-01-23T10:29:50Z"
    }
  ],
  "topImages": [
    {
      "imageUrl": "https://oaidalleapiprodscus.blob.core.windows.net/...",
      "prompt": "A futuristic cityscape...",
      "qualityScore": 8.5,
      "relevanceScore": 9.0,
      "overallScore": 8.75,
      "evaluationDetails": "...",
      "strengths": ["...", "..."],
      "weaknesses": ["..."]
    }
  ]
}
```

### POST /api/imagerefinement/export

Exports a refinement result: saves all images and metadata to a local folder.

**Request:**
```json
{
  "originalDescription": "...",
  "allGeneratedImages": [...],
  "topImages": [...],
  "totalImagesGenerated": 8,
  "imagesEvaluated": 8,
  "summary": "...",
  "processedAt": "2026-01-23T10:30:00Z"
}
```

**Query Parameters:**
- `folderName` (optional): Custom folder name. Defaults to `refinement_YYYY-MM-DD_HH-mm-ss`

**Response:**
```json
{
  "exportPath": "~/Downloads/ImageRefinementAssistant/refinement_2026-01-23_14-30-45",
  "imagesSaved": 8,
  "metadataPath": "~/Downloads/ImageRefinementAssistant/refinement_2026-01-23_14-30-45/metadata.json",
  "csvPath": "~/Downloads/ImageRefinementAssistant/refinement_2026-01-23_14-30-45/metadata.csv",
  "summaryPath": "~/Downloads/ImageRefinementAssistant/refinement_2026-01-23_14-30-45/summary.txt",
  "savedImages": [
    {
      "rank": 1,
      "fileName": "image_01_A_futuristic_cityscape.png",
      "filePath": "~/Downloads/ImageRefinementAssistant/.../image_01_A_futuristic_cityscape.png",
      "imageUrl": "https://...",
      "prompt": "A futuristic cityscape...",
      "qualityScore": 8.5,
      "relevanceScore": 9.0,
      "overallScore": 8.75,
      "isTopImage": true
    }
  ]
}
```

### GET /api/imagerefinement/health

Health check endpoint.

**Response:**
```json
{
  "status": "healthy",
  "service": "ImageRefinementAssistant"
}
```

## Usage Examples

### Web UI Usage

The easiest way to use the Image Refinement Assistant is through the web interface. Simply:
1. Enter prompts in the text area
2. Configure number of images to generate and top images to return
3. Optionally add custom evaluation criteria
4. Click "Generate & Evaluate Images"
5. Browse the gallery sorted by score with prompts and detailed feedback

### API Usage (Basic)

Generate 8 images and get the top 3:

```bash
curl -X POST "http://localhost:5001/api/imagerefinement/refine" \
  -H "Content-Type: application/json" \
  -H "X-API-Key: your-api-key" \
  -d '{
    "prompts": ["A modern office workspace with plants and natural lighting"],
    "numberOfImagesToGenerate": 8,
    "topImagesToReturn": 3
  }'
```

### Multiple Prompts

Generate images from multiple prompts:

```bash
curl -X POST "http://localhost:5001/api/imagerefinement/refine" \
  -H "Content-Type: application/json" \
  -H "X-API-Key: your-api-key" \
  -d '{
    "prompts": [
      "A minimalist logo design for a tech startup",
      "A vibrant poster design for a music festival",
      "A professional headshot background"
    ],
    "numberOfImagesToGenerate": 9,
    "topImagesToReturn": 3
  }'
```

### Custom Evaluation Criteria

Specify custom evaluation criteria:

```bash
curl -X POST "http://localhost:5001/api/imagerefinement/refine" \
  -H "Content-Type: application/json" \
  -H "X-API-Key: your-api-key" \
  -d '{
    "prompts": ["A book cover for a mystery novel"],
    "numberOfImagesToGenerate": 10,
    "topImagesToReturn": 3,
    "evaluationCriteria": "Focus on genre appropriateness, readability of title text, and marketability for mystery/thriller audience"
  }'
```

### High-Quality Images

Generate HD quality images:

```bash
curl -X POST "http://localhost:5001/api/imagerefinement/refine" \
  -H "Content-Type: application/json" \
  -H "X-API-Key: your-api-key" \
  -d '{
    "prompts": ["A professional product photography setup"],
    "numberOfImagesToGenerate": 6,
    "topImagesToReturn": 2,
    "quality": "hd",
    "size": "1792x1024"
  }'
```

### Python Example

```python
import requests

url = "http://localhost:5001/api/imagerefinement/refine"
headers = {
    "Content-Type": "application/json",
    "X-API-Key": "your-api-key"
}
data = {
    "prompts": [
        "A futuristic cityscape at sunset with flying cars and neon lights"
    ],
    "numberOfImagesToGenerate": 8,
    "topImagesToReturn": 3,
    "evaluationCriteria": "Focus on visual appeal, composition quality, and color harmony"
}

response = requests.post(url, json=data, headers=headers)
result = response.json()

print(f"Generated {result['totalImagesGenerated']} images")
print(f"Top {len(result['topImages'])} images selected:")
for idx, img in enumerate(result['topImages'], 1):
    print(f"{idx}. Score: {img['overallScore']:.2f}/10.0")
    print(f"   URL: {img['imageUrl']}")
    print(f"   Strengths: {', '.join(img['strengths'][:3])}")
```

## Evaluation Criteria

The AI evaluates images based on:

1. **Quality Score (0-10)**: Technical quality, visual appeal, composition, color harmony, detail level
2. **Relevance Score (0-10)**: How well the image matches the original prompt, accuracy, appropriateness
3. **Overall Score**: Average of quality and relevance scores

### Evaluation Factors

- Visual composition and balance
- Color palette and harmony
- Technical execution (sharpness, clarity)
- Adherence to prompt requirements
- Overall aesthetic appeal
- Professional quality

## Best Practices

1. **Prompt Quality**: Write clear, detailed prompts for better results
2. **Generation Count**: Generate 5-10 images for good variety without excessive cost
3. **Evaluation Criteria**: Provide specific criteria for domain-specific needs (e.g., "suitable for print", "web-optimized")
4. **Batch Processing**: Process multiple related prompts together for efficiency
5. **Review Top Candidates**: Always review the top images before final selection
6. **Iterative Refinement**: Use feedback from evaluations to refine prompts for subsequent runs

## Cost Considerations

- **DALL-E 3**: ~$0.04 per image (1024x1024, standard quality)
- **GPT-4 Vision**: ~$0.01-0.03 per image evaluation (depending on detail level)
- **Total per refinement**: ~$0.40-0.70 for 8 images (generation + evaluation)

**Cost Optimization Tips:**
- Use standard quality unless HD is required
- Generate fewer images if budget is constrained (minimum 5 recommended)
- Reuse evaluation criteria across similar batches
- Consider caching evaluations for identical prompts

## Integration Examples

### Design Workflow

```csharp
var request = new ImageRefinementRequest
{
    Prompts = new List<string>
    {
        "Modern logo design for SaaS company, minimalist style",
        "Brand identity mockup with logo on business card"
    },
    NumberOfImagesToGenerate = 10,
    TopImagesToReturn = 3,
    EvaluationCriteria = "Professional design, scalable, suitable for print and digital",
    Size = "1024x1024",
    Quality = "standard"
};

var result = await _refinementService.RefineImagesAsync(request);

// Present top 3 to designer for final selection
foreach (var topImage in result.TopImages)
{
    Console.WriteLine($"Score: {topImage.OverallScore:F2}");
    Console.WriteLine($"URL: {topImage.ImageUrl}");
    Console.WriteLine($"Strengths: {string.Join(", ", topImage.Strengths)}");
}
```

### Marketing Campaign

```csharp
var campaignPrompts = new List<string>
{
    "Social media post image for product launch, vibrant colors",
    "Email header banner for newsletter, professional design",
    "Website hero image, modern and engaging"
};

var result = await _refinementService.RefineImagesAsync(
    new ImageRefinementRequest
    {
        Prompts = campaignPrompts,
        NumberOfImagesToGenerate = 9,
        TopImagesToReturn = 3,
        EvaluationCriteria = "Brand consistency, visual impact, conversion optimization"
    });
```

## Related Documentation

- [DALL-E Image Generation](../advanced-features/dall-e.md) - Direct DALL-E usage
- [Vision API](../advanced-features/vision-api.md) - Image analysis capabilities
- [Setup Guide](../getting-started/01-setup.md) - Initial configuration
- [API Examples](../../samples/REST-API-Examples/) - Complete API examples

## Technical Details

### Image Generation

- Uses DALL-E 3 by default (supports DALL-E 2)
- Generates images sequentially (DALL-E 3 supports n=1 only)
- Includes 500ms delay between generations to avoid rate limiting
- Captures revised prompts from DALL-E for reference

### Image Evaluation

- Uses GPT-4 Vision (gpt-4-vision-preview) with high detail
- Parses JSON evaluation responses when available
- Falls back to text parsing if JSON parsing fails
- Extracts scores, strengths, and weaknesses automatically

### Ranking Algorithm

- Sorts by overall score (average of quality + relevance)
- Returns top N images based on `topImagesToReturn` parameter
- Includes all generated images in response for transparency

## Saving Images

### 📦 Export All (Recommended - Images + Metadata)

**The easiest way to save everything:** Use the "📦 Export All" button in the Web UI.

**What gets exported:**
- ✅ All generated images (saved as individual files)
- ✅ `metadata.json` - Complete metadata with all details (JSON format)
- ✅ `metadata.csv` - Spreadsheet-friendly format for easy viewing in Excel
- ✅ `summary.txt` - Human-readable summary with top images and scores

**Export location:**
- Default: `~/Downloads/ImageRefinementAssistant/refinement_YYYY-MM-DD_HH-mm-ss/`
- Custom: Can be configured in `appsettings.json` via `ImageRefinement:StoragePath`

**Folder structure:**
```
~/Downloads/ImageRefinementAssistant/
└── refinement_2026-01-23_14-30-45/
    ├── image_01_A_futuristic_cityscape.png
    ├── image_02_A_futuristic_cityscape.png
    ├── image_03_A_serene_mountain.png
    ├── ...
    ├── metadata.json          (Complete metadata)
    ├── metadata.csv           (Spreadsheet format)
    └── summary.txt            (Human-readable summary)
```

**How to use:**
1. Generate and evaluate images using the Web UI
2. Review the gallery
3. Click "📦 Export All (Images + Metadata)" button
4. Wait for export to complete (may take a minute for many images)
5. Check the export location shown in the success message

**API Usage:**
```bash
# Export refinement result
curl -X POST "http://localhost:5001/api/imagerefinement/export?folderName=my_custom_folder" \
  -H "Content-Type: application/json" \
  -H "X-API-Key: your-api-key" \
  -d @refinement_result.json
```

**Python Example:**
```python
import requests

# First, get refinement result
refine_response = requests.post(
    "http://localhost:5001/api/imagerefinement/refine",
    json={
        "prompts": ["A futuristic cityscape"],
        "numberOfImagesToGenerate": 8,
        "topImagesToReturn": 3
    },
    headers={"X-API-Key": "your-api-key"}
)
result = refine_response.json()

# Then export everything
export_response = requests.post(
    "http://localhost:5001/api/imagerefinement/export",
    json=result,
    headers={"X-API-Key": "your-api-key"}
)
export_info = export_response.json()

print(f"Exported to: {export_info['exportPath']}")
print(f"Saved {export_info['imagesSaved']} images")
print(f"Metadata: {export_info['metadataPath']}")
```

### Download Individual Images

**Each image in the gallery has download buttons:**
- Click "💾 Save Image" button below any image
- Or click the "⬇️ Download" button in the top-right corner of the image
- Images are saved with descriptive filenames like: `image_1_A_futuristic_cityscape.png`

**Important Notes:**
- **DALL-E URLs expire**: Image URLs from DALL-E are temporary and expire after some time (typically hours to days)
- **Download promptly**: Save images you want to keep as soon as possible
- **File naming**: Files are automatically named based on the prompt and image rank

### Manual Download (Alternative)

You can also download images manually:
1. Right-click on any image in the gallery
2. Select "Save image as..." or "Download image"
3. Choose your save location

## Troubleshooting

### Image URLs Expired

- **Issue**: Download fails with "404 Not Found" or "URL expired"
- **Solution**: DALL-E image URLs expire after some time. Use the "Export All" feature immediately after generation to save all images, or re-generate if needed

### Export Failed

- **Issue**: Export button shows error
- **Solution**: 
  - Check that the API is running
  - Verify you have write permissions to the storage directory
  - Check the storage path in `appsettings.json` is valid
  - Review API logs for specific error messages

### Low Quality Scores

- **Issue**: All images score below 7.0
- **Solution**: Refine prompts with more specific details, adjust evaluation criteria, or try different image sizes/quality settings

### Evaluation Failures

- **Issue**: Some images fail to evaluate
- **Solution**: Check API key permissions, verify Vision API access, review error logs for specific failures

### Rate Limiting

- **Issue**: Too many requests in short time
- **Solution**: Reduce `numberOfImagesToGenerate`, add delays between batches, or implement exponential backoff

### High Costs

- **Issue**: Costs are higher than expected
- **Solution**: Reduce generation count, use standard quality instead of HD, or batch similar prompts together

### API Key Not Found

- **Issue**: "API key not found" error
- **Solution**: 
  1. Load API key from `.env` file: `source scripts/setup-env.sh` (macOS/Linux) or `.\scripts\setup-env.ps1` (Windows)
  2. Verify it's set: `echo $OpenAI__ApiKey` (macOS/Linux) or `echo $env:OpenAI__ApiKey` (Windows PowerShell)
  3. Make sure you run the setup script in the same terminal session where you run `dotnet run`

### Web UI Can't Connect to API

- **Issue**: Web UI shows connection errors
- **Solution**: 
  1. Verify API is running on `http://localhost:5001` (check console output for actual port)
  2. Update `appsettings.json` in Web project if API is on a different port
  3. Check CORS is enabled in API (should be enabled by default)

## Export Features

### What Gets Exported

When you click "Export All", the system creates a folder with:

1. **All Images** - Each image saved as a separate file:
   - Named by rank and prompt: `image_01_A_futuristic_cityscape.png`
   - Original quality preserved
   - Organized in one folder

2. **metadata.json** - Complete metadata in JSON format:
   ```json
   {
     "exportDate": "2026-01-23T14:30:45Z",
     "originalDescription": "...",
     "totalImagesGenerated": 8,
     "images": [
       {
         "rank": 1,
         "fileName": "image_01_...",
         "prompt": "...",
         "qualityScore": 8.5,
         "relevanceScore": 9.0,
         "overallScore": 8.75,
         "isTopImage": true,
         "strengths": [...],
         "weaknesses": [...]
       }
     ]
   }
   ```

3. **metadata.csv** - Spreadsheet-friendly format:
   - Open in Excel, Google Sheets, or any spreadsheet app
   - Columns: Rank, FileName, Prompt, RevisedPrompt, QualityScore, RelevanceScore, OverallScore, IsTopImage, ImageUrl
   - Easy to sort, filter, and analyze

4. **summary.txt** - Human-readable summary:
   - Export date and overview
   - Top images with details
   - All images sorted by score
   - Quick reference for review

### Export Location

**Default:** `~/Downloads/ImageRefinementAssistant/refinement_YYYY-MM-DD_HH-mm-ss/`

**Custom:** Set in `appsettings.json`:
```json
{
  "ImageRefinement": {
    "StoragePath": "/path/to/your/storage"
  }
}
```

## Future Enhancements

Potential improvements:
- **Cloud storage integration**: Export directly to S3, Azure Blob, or Google Cloud Storage
- **Export templates**: Customizable export formats (PDF reports, HTML galleries)
- **Batch export**: Export multiple refinement sessions at once
- **Image gallery persistence**: Save refinement sessions for later review
- **Export scheduling**: Automatically export at specific times
- Parallel image generation (when API supports it)
- Custom scoring weights (e.g., prioritize quality over relevance)
- Image comparison mode (side-by-side evaluation)
- Prompt optimization suggestions based on results
- Caching of evaluations for identical prompts/images
