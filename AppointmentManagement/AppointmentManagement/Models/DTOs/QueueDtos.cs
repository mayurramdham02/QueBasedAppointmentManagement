using System.ComponentModel.DataAnnotations;

namespace AppointmentManagement.Models.DTOs
{
    /// <summary>
    /// Data Transfer Object for adding a patient to the queue
    /// </summary>
    public class AddPatientDto
    {
        /// <summary>
        /// Patient's name
        /// </summary>
        [Required(ErrorMessage = "Patient name is required")]
        [MaxLength(100, ErrorMessage = "Patient name cannot exceed 100 characters")]
        public string PatientName { get; set; } = null!;

        /// <summary>
        /// Description of symptoms
        /// </summary>
        [Required(ErrorMessage = "Symptom description is required")]
        [MaxLength(500, ErrorMessage = "Symptom description cannot exceed 500 characters")]
        public string Symptom { get; set; } = null!;

        /// <summary>
        /// Pain level (1-10)
        /// </summary>
        [Required(ErrorMessage = "Pain level is required")]
        [Range(1, 10, ErrorMessage = "Pain level must be between 1 and 10")]
        public int PainLevel { get; set; }
    }

    /// <summary>
    /// Data Transfer Object for toggling provider online status
    /// </summary>
    public class ToggleProviderStatusDto
    {
        /// <summary>
        /// Whether the provider should be online
        /// </summary>
        [Required]
        public bool IsOnline { get; set; }
    }

    /// <summary>
    /// Data Transfer Object for removing a patient from queue
    /// </summary>
    public class RemovePatientDto
    {
        /// <summary>
        /// ID of the provider accepting the patient
        /// </summary>
        [Required(ErrorMessage = "Provider ID is required")]
        public Guid ProviderId { get; set; }
    }
}
