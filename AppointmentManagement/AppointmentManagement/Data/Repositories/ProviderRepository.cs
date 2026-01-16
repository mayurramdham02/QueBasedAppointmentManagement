using AppointmentManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace AppointmentManagement.Data.Repositories
{
    /// <summary>
    /// Repository implementation for Provider operations.
    /// Handles all database interactions for providers.
    /// </summary>
    public class ProviderRepository : IProviderRepository
    {
        private readonly QueueDbContext _context;
        private readonly ILogger<ProviderRepository> _logger;

        public ProviderRepository(QueueDbContext context, ILogger<ProviderRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all providers
        /// </summary>
        public async Task<List<Provider>> GetAllAsync()
        {
            try
            {
                return await _context.Providers
                    .Include(p => p.AcceptedPatients)
                    .ToListAsync();
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
        public async Task<Provider?> GetByIdAsync(Guid id)
        {
            try
            {
                return await _context.Providers
                    .Include(p => p.AcceptedPatients)
                    .FirstOrDefaultAsync(p => p.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving provider with ID: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Get a provider by name
        /// </summary>
        public async Task<Provider?> GetByNameAsync(string name)
        {
            try
            {
                return await _context.Providers
                    .Include(p => p.AcceptedPatients)
                    .FirstOrDefaultAsync(p => p.ProviderName == name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving provider with name: {Name}", name);
                throw;
            }
        }

        /// <summary>
        /// Get count of online providers
        /// </summary>
        public async Task<int> GetOnlineCountAsync()
        {
            try
            {
                return await _context.Providers
                    .Where(p => p.IsOnline)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting online providers");
                throw;
            }
        }

        /// <summary>
        /// Update a provider
        /// </summary>
        public async Task UpdateAsync(Provider provider)
        {
            try
            {
                _context.Providers.Update(provider);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated provider: {Id} - {Name}", provider.Id, provider.ProviderName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating provider: {Id}", provider.Id);
                throw;
            }
        }

        /// <summary>
        /// Toggle provider online status
        /// </summary>
        public async Task ToggleOnlineStatusAsync(Guid providerId, bool isOnline)
        {
            try
            {
                var provider = await GetByIdAsync(providerId);
                if (provider == null)
                {
                    _logger.LogWarning("Provider not found: {Id}", providerId);
                    throw new KeyNotFoundException($"Provider with ID {providerId} not found");
                }

                provider.IsOnline = isOnline;
                provider.LastActivityTime = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Toggled provider {Id} online status to: {IsOnline}", providerId, isOnline);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling provider online status: {Id}", providerId);
                throw;
            }
        }
    }
}
