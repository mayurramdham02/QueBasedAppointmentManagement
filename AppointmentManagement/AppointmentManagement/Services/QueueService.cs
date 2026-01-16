using AppointmentManagement.Data.Repositories;
using AppointmentManagement.Hubs;
using AppointmentManagement.Models;
using AppointmentManagement.Models.Enums;
using Microsoft.AspNetCore.SignalR;

namespace AppointmentManagement.Services
{
    /// <summary>
    /// Service implementation for queue management business logic.
    /// Handles patient queue operations, wait time calculations, provider management, and real-time updates.
    /// </summary>
    public class QueueService : IQueueService
    {
        private readonly IQueueRepository _queueRepository;
        private readonly IProviderRepository _providerRepository;
        private readonly IHubContext<QueueHub> _hubContext;
        private readonly ILogger<QueueService> _logger;

        public QueueService(
            IQueueRepository queueRepository,
            IProviderRepository providerRepository,
            IHubContext<QueueHub> hubContext,
            ILogger<QueueService> logger)
        {
            _queueRepository = queueRepository;
            _providerRepository = providerRepository;
            _hubContext = hubContext;
            _logger = logger;
        }

        /// <summary>
        /// Add a new patient to the queue
        /// </summary>
        public async Task<QueueItem> AddPatientAsync(QueueItem queueItem)
        {
            try
            {
                _logger.LogInformation("Adding new patient to queue: {PatientName}", queueItem.PatientName);

                // Add patient to queue via repository
                var addedItem = await _queueRepository.AddAsync(queueItem);

                // Update wait times for all patients
                await UpdateWaitTimesAsync();

                // Broadcast queue update to all connected clients
                await BroadcastQueueUpdateAsync();

                _logger.LogInformation("Successfully added patient {Id} to queue", addedItem.Id);
                return addedItem;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding patient to queue: {PatientName}", queueItem.PatientName);
                throw;
            }
        }

        /// <summary>
        /// Remove a patient from the queue (accept by provider)
        /// </summary>
        public async Task RemovePatientAsync(Guid queueItemId, Guid providerId)
        {
            try
            {
                _logger.LogInformation("Provider {ProviderId} accepting patient {QueueItemId}", providerId, queueItemId);

                // Get the queue item
                var queueItem = await _queueRepository.GetByIdAsync(queueItemId);
                if (queueItem == null)
                {
                    _logger.LogWarning("Queue item not found: {QueueItemId}", queueItemId);
                    throw new KeyNotFoundException($"Queue item with ID {queueItemId} not found");
                }

                // Verify queue item is in Waiting status
                if (queueItem.Status != QueueStatus.Waiting)
                {
                    _logger.LogWarning("Queue item {QueueItemId} is not in Waiting status. Current status: {Status}", 
                        queueItemId, queueItem.Status);
                    throw new InvalidOperationException($"Queue item is not in Waiting status. Current status: {queueItem.Status}");
                }

                // Verify provider exists
                var provider = await _providerRepository.GetByIdAsync(providerId);
                if (provider == null)
                {
                    _logger.LogWarning("Provider not found: {ProviderId}", providerId);
                    throw new KeyNotFoundException($"Provider with ID {providerId} not found");
                }

                // Update queue item status
                queueItem.Status = QueueStatus.Accepted;
                queueItem.AcceptedByProviderId = providerId;
                await _queueRepository.UpdateAsync(queueItem);

                // Update wait times for remaining patients
                await UpdateWaitTimesAsync();

                // Broadcast queue update
                await BroadcastQueueUpdateAsync();

                _logger.LogInformation("Patient {QueueItemId} accepted by provider {ProviderId}", queueItemId, providerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing patient {QueueItemId} from queue", queueItemId);
                throw;
            }
        }

        /// <summary>
        /// Get all waiting patients in the queue
        /// </summary>
        public async Task<List<QueueItem>> GetQueueAsync()
        {
            try
            {
                return await _queueRepository.GetQueueAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving queue");
                throw;
            }
        }

        /// <summary>
        /// Get a specific queue item by ID
        /// </summary>
        public async Task<QueueItem?> GetQueueItemByIdAsync(Guid id)
        {
            try
            {
                return await _queueRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving queue item: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Update estimated wait times for all patients in queue
        /// </summary>
        public async Task UpdateWaitTimesAsync()
        {
            try
            {
                // Get count of online providers
                var activeProviders = await _providerRepository.GetOnlineCountAsync();

                // Update wait times via repository
                await _queueRepository.UpdateWaitTimesAsync(activeProviders);

                _logger.LogInformation("Updated wait times with {ActiveProviders} active providers", activeProviders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating wait times");
                throw;
            }
        }

        /// <summary>
        /// Toggle provider online/offline status
        /// </summary>
        public async Task ToggleProviderOnlineAsync(Guid providerId, bool isOnline)
        {
            try
            {
                _logger.LogInformation("Toggling provider {ProviderId} online status to: {IsOnline}", providerId, isOnline);

                // Toggle provider status via repository
                await _providerRepository.ToggleOnlineStatusAsync(providerId, isOnline);

                // Update wait times based on new provider count
                await UpdateWaitTimesAsync();

                // Broadcast queue update
                await BroadcastQueueUpdateAsync();

                _logger.LogInformation("Provider {ProviderId} is now {Status}", 
                    providerId, isOnline ? "online" : "offline");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling provider {ProviderId} online status", providerId);
                throw;
            }
        }

        /// <summary>
        /// Get all providers
        /// </summary>
        public async Task<List<Provider>> GetAllProvidersAsync()
        {
            try
            {
                return await _providerRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all providers");
                throw;
            }
        }

        /// <summary>
        /// Get a provider by ID
        /// </summary>
        public async Task<Provider?> GetProviderByIdAsync(Guid id)
        {
            try
            {
                return await _providerRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving provider: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Broadcast queue updates to all connected clients via SignalR
        /// </summary>
        public async Task BroadcastQueueUpdateAsync()
        {
            try
            {
                // Get current queue
                var queue = await GetQueueAsync();

                // Get all providers
                var providers = await GetAllProvidersAsync();

                // Broadcast to all clients
                await _hubContext.Clients.All.SendAsync("QueueUpdated", new
                {
                    Queue = queue,
                    Providers = providers,
                    Timestamp = DateTime.UtcNow
                });

                _logger.LogInformation("Broadcasted queue update to all clients. Queue size: {QueueSize}", queue.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting queue update");
                throw;
            }
        }
    }
}
