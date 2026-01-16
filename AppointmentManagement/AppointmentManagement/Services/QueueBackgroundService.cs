using AppointmentManagement.Models;

namespace AppointmentManagement.Services
{
    /// <summary>
    /// Background service that automatically adds ghost patients to the queue for demonstration.
    /// Runs periodic tasks: ghost patient generation and global clock updates.
    /// </summary>
    public class QueueBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<QueueBackgroundService> _logger;
        private readonly Random _random = new();

        // Sample data for ghost patient generation
        private readonly string[] _firstNames = { "John", "Jane", "Michael", "Sarah", "David", "Emily", "Robert", "Lisa", "James", "Maria" };
        private readonly string[] _lastNames = { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez" };
        private readonly string[] _symptoms = 
        { 
            "Severe headache", 
            "Chest pain", 
            "Abdominal pain", 
            "Difficulty breathing", 
            "High fever",
            "Persistent cough",
            "Dizziness",
            "Nausea and vomiting",
            "Back pain",
            "Allergic reaction"
        };

        public QueueBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<QueueBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// Main execution loop for background tasks
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("QueueBackgroundService started");

            // Wait 10 seconds before starting to allow app to initialize
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            // Create timers for periodic tasks
            using var ghostPatientTimer = new PeriodicTimer(TimeSpan.FromSeconds(30));
            using var globalClockTimer = new PeriodicTimer(TimeSpan.FromSeconds(60));

            // Start both tasks concurrently
            var ghostPatientTask = RunGhostPatientGeneratorAsync(ghostPatientTimer, stoppingToken);
            var globalClockTask = RunGlobalClockAsync(globalClockTimer, stoppingToken);

            await Task.WhenAll(ghostPatientTask, globalClockTask);
        }

        /// <summary>
        /// Periodically add ghost patients to the queue (every 30 seconds)
        /// </summary>
        private async Task RunGhostPatientGeneratorAsync(PeriodicTimer timer, CancellationToken stoppingToken)
        {
            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await AddGhostPatientAsync();
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Ghost patient generator stopped");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ghost patient generator");
            }
        }

        /// <summary>
        /// Periodically update wait times (every 60 seconds)
        /// </summary>
        private async Task RunGlobalClockAsync(PeriodicTimer timer, CancellationToken stoppingToken)
        {
            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await UpdateGlobalClockAsync();
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Global clock stopped");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in global clock");
            }
        }

        /// <summary>
        /// Add a random ghost patient to the queue
        /// </summary>
        private async Task AddGhostPatientAsync()
        {
            try
            {
                // Create a scoped service to access QueueService
                using var scope = _serviceProvider.CreateScope();
                var queueService = scope.ServiceProvider.GetRequiredService<IQueueService>();

                // Generate random patient data
                var firstName = _firstNames[_random.Next(_firstNames.Length)];
                var lastName = _lastNames[_random.Next(_lastNames.Length)];
                var symptom = _symptoms[_random.Next(_symptoms.Length)];
                var painLevel = _random.Next(1, 11); // 1-10

                var ghostPatient = new QueueItem
                {
                    PatientName = $"{firstName} {lastName} (Ghost)",
                    Symptom = symptom,
                    PainLevel = painLevel
                };

                await queueService.AddPatientAsync(ghostPatient);

                _logger.LogInformation("Added ghost patient: {PatientName} with pain level {PainLevel}", 
                    ghostPatient.PatientName, ghostPatient.PainLevel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding ghost patient");
            }
        }

        /// <summary>
        /// Update wait times for all patients (global clock tick)
        /// </summary>
        private async Task UpdateGlobalClockAsync()
        {
            try
            {
                // Create a scoped service to access QueueService
                using var scope = _serviceProvider.CreateScope();
                var queueService = scope.ServiceProvider.GetRequiredService<IQueueService>();

                await queueService.UpdateWaitTimesAsync();

                _logger.LogInformation("Global clock tick: Updated wait times");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in global clock update");
            }
        }

        /// <summary>
        /// Called when the service is stopping
        /// </summary>
        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("QueueBackgroundService is stopping");
            await base.StopAsync(stoppingToken);
        }
    }
}
