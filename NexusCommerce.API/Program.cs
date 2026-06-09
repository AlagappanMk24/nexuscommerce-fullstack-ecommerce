using NexusCommerce.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

#region Service Registration

/// <summary>
/// Registers all required services with the dependency injection container.
/// </summary>
/// <remarks>
/// Services are organized by concern using extension methods for better maintainability.
/// </remarks>
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddHealthChecks();
builder.Services.AddControllers();

// 3. Add Swagger/OpenAPI support
builder.Services.AddEndpointsApiExplorer(); // Necessary if using older Swagger tools
builder.Services.AddSwaggerGen();
#endregion
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enforce HTTPS redirection for security
app.UseHttpsRedirection();

// Apply Cross-Origin Resource Sharing policy
app.UseCors("AllowMyOrigin");

// Enable serving of static files from wwwroot
app.UseStaticFiles();

// Global exception handling middleware
app.UseGlobalExceptionHandling();

// Enable authentication middleware
app.UseAuthentication();

// Map API controller endpoints
app.MapControllers();

#region Post-Build Operations

/// <summary>
/// Executes post-build operations including database seeding and logging configuration.
/// </summary>
/// <remarks>
/// These operations run after the pipeline is configured but before the application starts.
/// </remarks>

// Run database seeding asynchronously
await app.SeedDatabaseAsync();

// Configure custom file-based logging
app.ConfigureCustomFileLogging();

#endregion

// Start the application
app.Run();