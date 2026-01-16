using AppointmentManagement.Models;
using AppointmentManagement.Models.DTOs;
using AppointmentManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentManagement.Controllers
{
    /// <summary>
    /// API Controller for provider management operations.
    /// Handles provider status, availability, and information retrieval.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ProviderController : ControllerBase
    {
        private readonly IQueueService _queueService;
        private readonly ILogger<ProviderController> _logger;

        public ProviderController(IQueueService queueService, ILogger<ProviderController> logger)
        {
            _queueService = queueService;
            _logger = logger;
        }

        /// <summary>
        /// Get all providers
        /// </summary>
        /// <returns>List of all providers</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<Provider>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllProviders()
        {
            try
            {
                _logger.LogInformation("Retrieving all providers");
                var providers = await _queueService.GetAllProvidersAsync();
                return Ok(providers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving providers");
                return StatusCode(500, new { error = "An error occurred while retrieving providers" });
            }
        }

        /// <summary>
        /// Get a specific provider by ID
        /// </summary>
        /// <param name="id">Provider ID</param>
        /// <returns>Provider details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Provider), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProvider(Guid id)
        {
            try
            {
                _logger.LogInformation("Retrieving provider: {Id}", id);
                var provider = await _queueService.GetProviderByIdAsync(id);

                if (provider == null)
                {
                    _logger.LogWarning("Provider not found: {Id}", id);
                    return NotFound(new { error = $"Provider with ID {id} not found" });
                }

                return Ok(provider);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving provider: {Id}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving the provider" });
            }
        }

        /// <summary>
        /// Toggle provider online/offline status
        /// </summary>
        /// <param name="id">Provider ID</param>
        /// <param name="dto">Status information</param>
        /// <returns>Success response</returns>
        [HttpPut("{id}/toggle-online")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ToggleOnline(Guid id, [FromBody] ToggleProviderStatusDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for ToggleOnline request");
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Toggling provider {Id} online status to: {IsOnline}", id, dto.IsOnline);

                await _queueService.ToggleProviderOnlineAsync(id, dto.IsOnline);

                return Ok(new 
                { 
                    message = $"Provider status updated successfully",
                    providerId = id,
                    isOnline = dto.IsOnline
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Provider not found while toggling status");
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling provider online status");
                return StatusCode(500, new { error = "An error occurred while updating provider status" });
            }
        }
    }
}
