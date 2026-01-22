using Microsoft.AspNetCore.SignalR;

namespace RetroAnalyzer.Api.Hubs;

/// <summary>
/// SignalR hub for real-time retrospective analysis updates
/// </summary>
public class RetroHub : Hub
{
    private readonly ILogger<RetroHub> _logger;

    public RetroHub(ILogger<RetroHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Join a retrospective room
    /// </summary>
    public async Task JoinRoom(string roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        _logger.LogInformation("Client {ConnectionId} joined room {RoomId}", Context.ConnectionId, roomId);
        await Clients.Group(roomId).SendAsync("UserJoined", Context.ConnectionId);
    }

    /// <summary>
    /// Leave a retrospective room
    /// </summary>
    public async Task LeaveRoom(string roomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
        _logger.LogInformation("Client {ConnectionId} left room {RoomId}", Context.ConnectionId, roomId);
        await Clients.Group(roomId).SendAsync("UserLeft", Context.ConnectionId);
    }
}
