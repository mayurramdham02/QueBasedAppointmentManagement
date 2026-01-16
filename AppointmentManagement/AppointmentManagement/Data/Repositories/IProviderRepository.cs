using AppointmentManagement.Models;

namespace AppointmentManagement.Data.Repositories
{
    /// <summary>
    /// Repository interface for Provider operations.
    /// Provides abstraction for data access layer.
    /// </summary>
    public interface IProviderRepository
    {
        /// <summary>
        /// Get all providers
        /// </summary>
        Task<List<Provider>> GetAllAsync();

        /// <summary>
        /// Get a provider by ID
        /// </summary>
        Task<Provider?> GetByIdAsync(Guid id);

        /// <summary>
        /// Get a provider by name
        /// </summary>
        Task<Provider?> GetByNameAsync(string name);

        /// <summary>
        /// Get count of online providers
        /// </summary>
        Task<int> GetOnlineCountAsync();

        /// <summary>
        /// Update a provider
        /// </summary>
        Task UpdateAsync(Provider provider);

        /// <summary>
        /// Toggle provider online status
        /// </summary>
        Task ToggleOnlineStatusAsync(Guid providerId, bool isOnline);
    }
}
