using Microsoft.AspNetCore.SignalR;

namespace AppointmentManagement.Hubs
{
    /// <summary>
    /// SignalR Hub for real-time queue updates.
    /// Enables bidirectional communication between server and clients.
    /// </summary>
    public class QueueHub : Hub
    {
        private readonly ILogger<QueueHub> _logger;

        public QueueHub(ILogger<QueueHub> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Called when a client connects to the hub
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Called when a client disconnects from the hub
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
            
            if (exception != null)
            {
                _logger.LogError(exception, "Client disconnected with error: {ConnectionId}", Context.ConnectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Allow clients to join a specific group (e.g., "QueueGroup")
        /// </summary>
        public async Task JoinQueueGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "QueueGroup");
            _logger.LogInformation("Client {ConnectionId} joined QueueGroup", Context.ConnectionId);
        }

        /// <summary>
        /// Allow clients to leave the queue group
        /// </summary>
        public async Task LeaveQueueGroup()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "QueueGroup");
            _logger.LogInformation("Client {ConnectionId} left QueueGroup", Context.ConnectionId);
        }
    }
}
