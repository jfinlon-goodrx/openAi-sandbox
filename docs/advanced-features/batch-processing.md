# Batch Processing

## Overview

The Batch API allows you to process multiple requests asynchronously at a lower cost (50% discount).

## Usage

### Create Batch

```csharp
var batchService = new BatchService(httpClient, config, logger);

var requests = new List<BatchRequest>
{
    new() 
    { 
        CustomId = "req1",
        Body = new ChatCompletionRequest { /* ... */ }
    },
    new() 
    { 
        CustomId = "req2",
        Body = new ChatCompletionRequest { /* ... */ }
    }
};

var batch = await batchService.CreateBatchAsync(requests);
```

### Monitor Progress

```csharp
var status = await batchService.GetBatchStatusAsync(batch.Id);

// Status can be: validating, in_progress, finalizing, completed, failed
if (status.Status == "completed")
{
    var results = await batchService.GetBatchResultsAsync(status.OutputFileId!);
}
```

## Use Cases

- Processing multiple manuscripts for review
- Generating patient education for multiple medications
- Batch ad copy generation
- Large-scale content analysis

## Benefits

- 50% cost reduction
- No rate limits
- Asynchronous processing
- Suitable for large volumes

## Best Practices

1. Use for non-urgent, high-volume tasks
2. Monitor batch status periodically
3. Handle errors appropriately
4. Store batch IDs for tracking
