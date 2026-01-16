using System.ComponentModel.DataAnnotations;

namespace AppointmentManagement.Models
{
    /// <summary>
    /// Represents a healthcare provider.
    /// Tracks provider availability, online status, and accepted patients.
    /// </summary>
    public class Provider
    {
        /// <summary>
        /// Unique identifier for the provider
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Provider's name (must be unique)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string ProviderName { get; set; } = null!;

        /// <summary>
        /// Indicates whether the provider is currently online/available
        /// </summary>
        [Required]
        public bool IsOnline { get; set; } = false;

        /// <summary>
        /// Timestamp of the provider's last activity (nullable)
        /// </summary>
        public DateTime? LastActivityTime { get; set; }

        /// <summary>
        /// Navigation property for patients accepted by this provider
        /// </summary>
        public List<QueueItem> AcceptedPatients { get; set; } = new();

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
