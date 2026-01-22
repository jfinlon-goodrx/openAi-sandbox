# SignalR Real-Time Communication Guide

Complete guide to implementing real-time features with SignalR.

## Overview

SignalR enables real-time bidirectional communication between server and clients, perfect for:
- Live updates during long-running operations
- Collaborative editing
- Real-time notifications
- Progress tracking

## Setup

### 1. Add SignalR to Project

```csharp
builder.Services.AddSignalR();
```

### 2. Map Hub

```csharp
app.MapHub<RetroHub>("/retroHub");
```

### 3. Create Hub

```csharp
public class RetroHub : Hub
{
    public async Task JoinRoom(string roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
    }
}
```

## Usage Patterns

### Server-Side: Sending Messages

```csharp
public class RetroController : ControllerBase
{
    private readonly IHubContext<RetroHub> _hubContext;

    [HttpPost("analyze")]
    public async Task Analyze([FromBody] Request request)
    {
        // Send to specific group
        await _hubContext.Clients.Group(request.RoomId)
            .SendAsync("ProgressUpdate", new { Message = "Processing..." });

        // Send to specific connection
        await _hubContext.Clients.Client(connectionId)
            .SendAsync("Update", data);

        // Send to all clients
        await _hubContext.Clients.All
            .SendAsync("Broadcast", message);
    }
}
```

### Client-Side: JavaScript

```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/retroHub")
    .build();

// Start connection
await connection.start();

// Join room
await connection.invoke("JoinRoom", roomId);

// Listen for messages
connection.on("ProgressUpdate", (data) => {
    console.log("Progress:", data.message);
    updateUI(data);
});

connection.on("AnalysisComplete", (result) => {
    displayResults(result);
});

// Send messages
await connection.invoke("SendMessage", message);
```

### Client-Side: C# (.NET)

```csharp
var connection = new HubConnectionBuilder()
    .WithUrl("https://localhost:5001/retroHub")
    .Build();

await connection.StartAsync();

await connection.InvokeAsync("JoinRoom", roomId);

connection.On<ProgressData>("ProgressUpdate", data =>
{
    Console.WriteLine($"Progress: {data.Message}");
});

connection.On<AnalysisResult>("AnalysisComplete", result =>
{
    DisplayResults(result);
});
```

## Real-Time Use Cases

### 1. Live Code Review

```csharp
// Server
await _hubContext.Clients.Group(reviewId)
    .SendAsync("ReviewProgress", new { 
        Status = "Analyzing code...",
        Progress = 50 
    });
```

### 2. Meeting Transcription

```csharp
// Stream transcription results in real-time
foreach (var chunk in transcriptionChunks)
{
    await _hubContext.Clients.Group(meetingId)
        .SendAsync("TranscriptChunk", chunk);
}
```

### 3. Requirements Processing

```csharp
// Send progress updates
await _hubContext.Clients.Group(documentId)
    .SendAsync("ProcessingUpdate", new {
        Step = "Generating user stories",
        Progress = 75
    });
```

## Best Practices

### 1. Connection Management

- Handle reconnection automatically
- Clean up connections on disconnect
- Use groups for room-based communication

### 2. Error Handling

```javascript
connection.onclose((error) => {
    if (error) {
        console.error("Connection closed with error:", error);
    }
    // Attempt to reconnect
    startConnection();
});
```

### 3. Performance

- Use groups instead of individual connections when possible
- Batch updates when sending many messages
- Use compression for large payloads

### 4. Security

- Authenticate connections
- Authorize group access
- Validate all inputs

## Authentication

```csharp
public class RetroHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            Context.Abort();
            return;
        }
        await base.OnConnectedAsync();
    }
}
```

## Resources

- [SignalR Documentation](https://learn.microsoft.com/en-us/aspnet/core/signalr/introduction)
- [SignalR JavaScript Client](https://learn.microsoft.com/en-us/aspnet/core/signalr/javascript-client)
