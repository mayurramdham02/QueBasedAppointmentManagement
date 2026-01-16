namespace AppointmentManagement.Models.Enums
{
    /// <summary>
    /// Represents the current status of a queue item
    /// </summary>
    public enum QueueStatus
    {
        /// <summary>
        /// Patient is waiting in the queue
        /// </summary>
        Waiting = 0,

        /// <summary>
        /// Patient has been accepted by a provider
        /// </summary>
        Accepted = 1,

        /// <summary>
        /// Patient has been removed from the queue
        /// </summary>
        Removed = 2,

        /// <summary>
        /// Patient consultation is in progress
        /// </summary>
        InProgress = 3,

        /// <summary>
        /// Patient consultation is completed
        /// </summary>
        Completed = 4
    }
}
