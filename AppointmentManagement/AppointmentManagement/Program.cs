using AppointmentManagement.Data;
using AppointmentManagement.Data.Repositories;
using AppointmentManagement.Hubs;
using AppointmentManagement.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

// ===== Configure Serilog =====
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build())
    .CreateLogger();

try
{
    Log.Information("Starting Urgent Care Queue API");

    var builder = WebApplication.CreateBuilder(args);

    // ===== Configure Serilog =====
    builder.Host.UseSerilog();

    // ===== Add Database Context =====
    builder.Services.AddDbContext<QueueDbContext>(options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            sqlOptions => sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null
            )
        )
    );

    // ===== Register Repositories =====
    builder.Services.AddScoped<IQueueRepository, QueueRepository>();
    builder.Services.AddScoped<IProviderRepository, ProviderRepository>();

    // ===== Register Services =====
    builder.Services.AddScoped<IQueueService, QueueService>();

    // ===== Register Background Services =====
    builder.Services.AddHostedService<QueueBackgroundService>();

    // ===== Add SignalR =====
    builder.Services.AddSignalR(options =>
    {
        options.EnableDetailedErrors = builder.Environment.IsDevelopment();
        options.KeepAliveInterval = TimeSpan.FromSeconds(15);
        options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    });

    // ===== Add CORS =====
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAngularApp", policy =>
        {
            policy.WithOrigins(
                    "http://localhost:4200",
                    "https://localhost:4200",
                    "http://localhost:5173",
                    "https://localhost:5173"
                )
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials(); // Required for SignalR
        });
    });

    // ===== Add Controllers =====
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            // Configure JSON serialization - Use CamelCase (standard for JS/TS)
            options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment();
            // Fix for "A possible object cycle was detected" error
            options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
            // Serialize enums as strings
            options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        });

    // ===== Add API Documentation =====
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "Urgent Care Queue API",
            Version = "v1",
            Description = "API for managing urgent care patient queue with real-time updates",
            Contact = new Microsoft.OpenApi.Models.OpenApiContact
            {
                Name = "NeuEu Medical",
                Email = "support@neueumedical.com"
            }
        });

        // Include XML comments if available
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }
    });

    // ===== Build Application =====
    var app = builder.Build();

    // ===== Apply Migrations Automatically (Development Only) =====
    if (app.Environment.IsDevelopment())
    {
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<QueueDbContext>();
            try
            {
                Log.Information("Applying database migrations...");
                dbContext.Database.Migrate();
                Log.Information("Database migrations applied successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error applying database migrations");
            }
        }
    }

    // ===== Configure HTTP Request Pipeline =====

    // Use Serilog request logging
    app.UseSerilogRequestLogging();

    // Enable CORS
    app.UseCors("AllowAngularApp");

    // Enable Swagger in Development
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Urgent Care Queue API v1");
            // Swagger UI will be at /swagger
        });
    }

    // HTTPS Redirection
    app.UseHttpsRedirection();

    // Authentication & Authorization (if needed in future)
    app.UseAuthorization();

    // Map Controllers
    app.MapControllers();

    // Map SignalR Hub
    app.MapHub<QueueHub>("/queueHub");

    // ===== Run Application =====
    var urls = builder.Configuration["ASPNETCORE_URLS"] ?? 
               app.Configuration.GetValue<string>("applicationUrl") ?? 
               "https://localhost:7062;http://localhost:5259";
    
    Log.Information("Urgent Care Queue API started successfully");
    Log.Information("Application URLs: {Urls}", urls);
    Log.Information("Swagger UI: {SwaggerUrl}", $"{urls.Split(';')[0]}/swagger");
    Log.Information("SignalR Hub: {HubUrl}", $"{urls.Split(';')[0]}/queueHub");
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
