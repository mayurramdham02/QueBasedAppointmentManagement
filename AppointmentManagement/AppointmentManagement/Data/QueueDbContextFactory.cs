using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AppointmentManagement.Data
{
    /// <summary>
    /// Design-time factory for QueueDbContext.
    /// Required for EF Core migrations to work properly.
    /// </summary>
    public class QueueDbContextFactory : IDesignTimeDbContextFactory<QueueDbContext>
    {
        public QueueDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<QueueDbContext>();
            
            // Use the connection string from appsettings.json
            optionsBuilder.UseSqlServer(
                "Server=SDN-127\\SQLEXPRESS22;Database=QueManagement;Trusted_connection=true;TrustServerCertificate=True"
            );

            return new QueueDbContext(optionsBuilder.Options);
        }
    }
}
