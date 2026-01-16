using AppointmentManagement.Models;
using AppointmentManagement.Models.DTOs;
using AppointmentManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentManagement.Controllers
{
    /// <summary>
    /// API Controller for queue management operations.
    /// Handles patient queue operations: add, remove, and retrieve queue.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class QueueController : ControllerBase
    {
        private readonly IQueueService _queueService;
        private readonly ILogger<QueueController> _logger;

        public QueueController(IQueueService queueService, ILogger<QueueController> logger)
        {
            _queueService = queueService;
            _logger = logger;
        }

        /// <summary>
        /// Add a new patient to the queue
        /// </summary>
        /// <param name="dto">Patient information</param>
        /// <returns>Created queue item</returns>
        [HttpPost("add-patient")]
        [ProducesResponseType(typeof(QueueItem), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddPatient([FromBody] AddPatientDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for AddPatient request");
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Adding patient to queue: {PatientName}", dto.PatientName);

                // Map DTO to entity
                var queueItem = new QueueItem
                {
                    PatientName = dto.PatientName,
                    Symptom = dto.Symptom,
                    PainLevel = dto.PainLevel
                };

                var result = await _queueService.AddPatientAsync(queueItem);

                return CreatedAtAction(
                    nameof(GetQueueItem), 
                    new { id = result.Id }, 
                    result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding patient to queue");
                return StatusCode(500, new { error = "An error occurred while adding patient to queue" });
            }
        }

        /// <summary>
        /// Get all patients currently waiting in the queue
        /// </summary>
        /// <returns>List of waiting patients</returns>
        [HttpGet("get-queue")]
        [ProducesResponseType(typeof(List<QueueItem>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetQueue()
        {
            try
            {
                _logger.LogInformation("Retrieving current queue");
                var queue = await _queueService.GetQueueAsync();
                return Ok(queue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving queue");
                return StatusCode(500, new { error = "An error occurred while retrieving the queue" });
            }
        }

        /// <summary>
        /// Get a specific queue item by ID
        /// </summary>
        /// <param name="id">Queue item ID</param>
        /// <returns>Queue item details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(QueueItem), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetQueueItem(Guid id)
        {
            try
            {
                _logger.LogInformation("Retrieving queue item: {Id}", id);
                var queueItem = await _queueService.GetQueueItemByIdAsync(id);

                if (queueItem == null)
                {
                    _logger.LogWarning("Queue item not found: {Id}", id);
                    return NotFound(new { error = $"Queue item with ID {id} not found" });
                }

                return Ok(queueItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving queue item: {Id}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving the queue item" });
            }
        }

        /// <summary>
        /// Remove a patient from the queue (accept by provider)
        /// </summary>
        /// <param name="id">Queue item ID</param>
        /// <param name="dto">Provider information</param>
        /// <returns>Success response</returns>
        [HttpPost("{id}/remove")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemovePatient(Guid id, [FromBody] RemovePatientDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for RemovePatient request");
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Removing patient {Id} from queue by provider {ProviderId}", 
                    id, dto.ProviderId);

                await _queueService.RemovePatientAsync(id, dto.ProviderId);

                return Ok(new { message = "Patient successfully removed from queue" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found while removing patient");
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while removing patient");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing patient from queue");
                return StatusCode(500, new { error = "An error occurred while removing patient from queue" });
            }
        }

        /// <summary>
        /// Update wait times for all patients in queue (manual trigger)
        /// </summary>
        /// <returns>Success response</returns>
        [HttpPost("update-wait-times")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateWaitTimes()
        {
            try
            {
                _logger.LogInformation("Manually updating wait times");
                await _queueService.UpdateWaitTimesAsync();
                return Ok(new { message = "Wait times updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating wait times");
                return StatusCode(500, new { error = "An error occurred while updating wait times" });
            }
        }
    }
}
