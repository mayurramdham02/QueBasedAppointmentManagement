using AppointmentManagement.Models;
using AppointmentManagement.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace AppointmentManagement.Data.Repositories
{
    /// <summary>
    /// Repository implementation for QueueItem operations.
    /// Handles all database interactions for queue items.
    /// </summary>
    public class QueueRepository : IQueueRepository
    {
        private readonly QueueDbContext _context;
        private readonly ILogger<QueueRepository> _logger;

        public QueueRepository(QueueDbContext context, ILogger<QueueRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all queue items with optional status filtering
        /// </summary>
        public async Task<List<QueueItem>> GetAllAsync(string? status = null)
        {
            try
            {
                var query = _context.QueueItems
                    .Include(q => q.AcceptedByProvider)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(status) && Enum.TryParse<QueueStatus>(status, true, out var parsedStatus))
                {
                    query = query.Where(q => q.Status == parsedStatus);
                }

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving queue items with status: {Status}", status);
                throw;
            }
        }

        /// <summary>
        /// Get queue items ordered by join time (waiting patients only)
        /// </summary>
        public async Task<List<QueueItem>> GetQueueAsync()
        {
            try
            {
                return await _context.QueueItems
                    .Where(q => q.Status == QueueStatus.Waiting)
                    .OrderBy(q => q.JoinTime)
                    .Include(q => q.AcceptedByProvider)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving queue");
                throw;
            }
        }

        /// <summary>
        /// Get a queue item by ID
        /// </summary>
        public async Task<QueueItem?> GetByIdAsync(Guid id)
        {
            try
            {
                return await _context.QueueItems
                    .Include(q => q.AcceptedByProvider)
                    .FirstOrDefaultAsync(q => q.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving queue item with ID: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Add a new queue item
        /// </summary>
        public async Task<QueueItem> AddAsync(QueueItem queueItem)
        {
            try
            {
                queueItem.Id = Guid.NewGuid();
                queueItem.JoinTime = DateTime.UtcNow;
                queueItem.CreatedAt = DateTime.UtcNow;
                queueItem.UpdatedAt = DateTime.UtcNow;

                await _context.QueueItems.AddAsync(queueItem);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Added new queue item: {Id} - {Name}", queueItem.Id, queueItem.PatientName);
                return queueItem;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding queue item: {Name}", queueItem.PatientName);
                throw;
            }
        }

        /// <summary>
        /// Update an existing queue item
        /// </summary>
        public async Task UpdateAsync(QueueItem queueItem)
        {
            try
            {
                _context.QueueItems.Update(queueItem);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated queue item: {Id}", queueItem.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating queue item: {Id}", queueItem.Id);
                throw;
            }
        }

        /// <summary>
        /// Delete a queue item
        /// </summary>
        public async Task DeleteAsync(Guid id)
        {
            try
            {
                var queueItem = await GetByIdAsync(id);
                if (queueItem != null)
                {
                    _context.QueueItems.Remove(queueItem);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Deleted queue item: {Id}", id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting queue item: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Get count of patients ahead in queue
        /// </summary>
        public async Task<int> GetPatientsAheadCountAsync(DateTime joinTime)
        {
            try
            {
                return await _context.QueueItems
                    .Where(q => q.Status == QueueStatus.Waiting && q.JoinTime < joinTime)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting patients ahead for join time: {JoinTime}", joinTime);
                throw;
            }
        }

        /// <summary>
        /// Update wait times for all waiting patients based on active providers
        /// </summary>
        public async Task UpdateWaitTimesAsync(int activeProviders)
        {
            try
            {
                var queue = await GetQueueAsync();
                
                // Calculate wait time: (position in queue * 15 minutes) / active providers
                // If no providers are online, use 1 to avoid division by zero
                int providerCount = activeProviders > 0 ? activeProviders : 1;

                for (int i = 0; i < queue.Count; i++)
                {
                    queue[i].EstimatedWaitTimeMinutes = (i * 15) / providerCount;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Updated wait times for {Count} patients with {Providers} active providers", 
                    queue.Count, activeProviders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating wait times");
                throw;
            }
        }
    }
}
