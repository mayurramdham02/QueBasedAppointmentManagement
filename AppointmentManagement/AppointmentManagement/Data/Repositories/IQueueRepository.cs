using AppointmentManagement.Models;

namespace AppointmentManagement.Data.Repositories
{
    /// <summary>
    /// Repository interface for QueueItem operations.
    /// Provides abstraction for data access layer.
    /// </summary>
    public interface IQueueRepository
    {
        /// <summary>
        /// Get all queue items with optional filtering by status
        /// </summary>
        Task<List<QueueItem>> GetAllAsync(string? status = null);

        /// <summary>
        /// Get queue items ordered by join time (for queue display)
        /// </summary>
        Task<List<QueueItem>> GetQueueAsync();

        /// <summary>
        /// Get a queue item by ID
        /// </summary>
        Task<QueueItem?> GetByIdAsync(Guid id);

        /// <summary>
        /// Add a new queue item
        /// </summary>
        Task<QueueItem> AddAsync(QueueItem queueItem);

        /// <summary>
        /// Update an existing queue item
        /// </summary>
        Task UpdateAsync(QueueItem queueItem);

        /// <summary>
        /// Delete a queue item
        /// </summary>
        Task DeleteAsync(Guid id);

        /// <summary>
        /// Get count of patients ahead in queue for a specific patient
        /// </summary>
        Task<int> GetPatientsAheadCountAsync(DateTime joinTime);

        /// <summary>
        /// Update wait times for all waiting patients
        /// </summary>
        Task UpdateWaitTimesAsync(int activeProviders);
    }
}
