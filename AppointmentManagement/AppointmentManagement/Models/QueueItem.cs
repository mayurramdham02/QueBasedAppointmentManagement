using System.ComponentModel.DataAnnotations;
using AppointmentManagement.Models.Enums;

namespace AppointmentManagement.Models
{
    /// <summary>
    /// Represents a patient in the queue.
    /// Tracks patient information, symptoms, wait times, and queue status.
    /// </summary>
    public class QueueItem
    {
        /// <summary>
        /// Unique identifier for the queue item
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Patient's name
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string PatientName { get; set; } = null!;

        /// <summary>
        /// Description of patient's symptoms
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Symptom { get; set; } = null!;

        /// <summary>
        /// Pain level on a scale of 1-10
        /// </summary>
        [Required]
        [Range(1, 10, ErrorMessage = "Pain level must be between 1 and 10")]
        public int PainLevel { get; set; }

        /// <summary>
        /// Timestamp when patient joined the queue
        /// </summary>
        [Required]
        public DateTime JoinTime { get; set; }

        /// <summary>
        /// Estimated wait time in minutes (nullable, calculated dynamically)
        /// </summary>
        public int? EstimatedWaitTimeMinutes { get; set; }

        /// <summary>
        /// Current status of the queue item using enum
        /// </summary>
        [Required]
        public QueueStatus Status { get; set; } = QueueStatus.Waiting;

        /// <summary>
        /// Foreign key to the provider who accepted this patient (nullable)
        /// </summary>
        public Guid? AcceptedByProviderId { get; set; }

        /// <summary>
        /// Navigation property to the provider who accepted this patient
        /// </summary>
        public Provider? AcceptedByProvider { get; set; }

        /// <summary>
        /// Timestamp when this record was created
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Timestamp when this record was last updated
        /// </summary>
        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
