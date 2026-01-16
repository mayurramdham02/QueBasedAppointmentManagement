using AppointmentManagement.Models;

namespace AppointmentManagement.Services
{
    /// <summary>
    /// Service interface for queue management business logic.
    /// Handles patient queue operations, wait time calculations, and provider management.
    /// </summary>
    public interface IQueueService
    {
        /// <summary>
        /// Add a new patient to the queue
        /// </summary>
        Task<QueueItem> AddPatientAsync(QueueItem queueItem);

        /// <summary>
        /// Remove a patient from the queue (accept by provider)
        /// </summary>
        Task RemovePatientAsync(Guid queueItemId, Guid providerId);

        /// <summary>
        /// Get all waiting patients in the queue
        /// </summary>
        Task<List<QueueItem>> GetQueueAsync();

        /// <summary>
        /// Get a specific queue item by ID
        /// </summary>
        Task<QueueItem?> GetQueueItemByIdAsync(Guid id);

        /// <summary>
        /// Update estimated wait times for all patients in queue
        /// </summary>
        Task UpdateWaitTimesAsync();

        /// <summary>
        /// Toggle provider online/offline status
        /// </summary>
        Task ToggleProviderOnlineAsync(Guid providerId, bool isOnline);

        /// <summary>
        /// Get all providers
        /// </summary>
        Task<List<Provider>> GetAllProvidersAsync();

        /// <summary>
        /// Get a provider by ID
        /// </summary>
        Task<Provider?> GetProviderByIdAsync(Guid id);

        /// <summary>
        /// Broadcast queue updates to all connected clients via SignalR
        /// </summary>
        Task BroadcastQueueUpdateAsync();
    }
}
