using SchoolAdmission.Api.BackgroundServices;
using SchoolAdmission.Api.Data;
using SchoolAdmission.Api.Middleware;
using SchoolAdmission.Api.Services;

using Microsoft.EntityFrameworkCore;
using SchoolAdmission.Api.BackgroundServices;
using SchoolAdmission.Api.Data;
using SchoolAdmission.Api.Middleware;
using SchoolAdmission.Api.Repositories;
using SchoolAdmission.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddDbContext<AppDbContext>((sp, options) =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString, npgsql =>
    {
        npgsql.MigrationsAssembly("SchoolAdmission.Api");
        npgsql.CommandTimeout(30);
    });
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

builder.Services.AddHealthChecks();

var allowedOrigins = builder.Configuration["CORS_ALLOWED_ORIGINS"]
    ?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    ?? Array.Empty<string>();
if (allowedOrigins.Length == 0)
{
    allowedOrigins = new[] { "http://localhost:3000" };
}
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", p => p
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod());
});

builder.Services.AddSingleton(TimeProvider.System);
var holdDurationSeconds = int.Parse(builder.Configuration["HOLD_DURATION_SECONDS"] ?? "600");
builder.Services.AddSingleton(typeof(int), holdDurationSeconds);
builder.Services.AddScoped<IAdmissionService, AdmissionService>();
builder.Services.AddScoped<IClassRepository, ClassRepository>();
builder.Services.AddScoped<IHoldRepository, HoldRepository>();
builder.Services.AddHostedService<HoldExpirationService>();

builder.WebHost.UseUrls("http://0.0.0.0:8080");

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapHealthChecks("/health");

app.UseCors("CorsPolicy");
app.MapControllers();

app.MapGet("/", () => Results.Ok(new { name = "School Admission System", version = "1.0.0", status = "running" }));

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    for (var attempt = 1; attempt <= 30; attempt++)
    {
        try
        {
            context.Database.Migrate();
            await EnsureSchemaAsync(context);
            logger.LogInformation("Database schema is ready.");
            break;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Database initialization attempt {Attempt} failed, retrying in 2s...", attempt);
            if (attempt == 30) throw;
            await Task.Delay(2000);
        }
    }
}

app.Run();

static async Task EnsureSchemaAsync(AppDbContext context)
{
    // Idempotent safety net: ensures tables exist even if EF migration discovery
    // does not apply them at runtime. Migration files remain the source of truth.
    await context.Database.ExecuteSqlRawAsync(@"
CREATE TABLE IF NOT EXISTS ""AdmissionClasses"" (
    ""Id"" text NOT NULL PRIMARY KEY,
    ""Name"" character varying(200) NOT NULL,
    ""TotalSeats"" integer NOT NULL,
    ""CreatedAt"" timestamp with time zone NOT NULL,
    ""UpdatedAt"" timestamp with time zone NOT NULL
);
CREATE TABLE IF NOT EXISTS ""AdmissionHolds"" (
    ""Id"" text NOT NULL PRIMARY KEY,
    ""ClassId"" text NOT NULL,
    ""StudentId"" character varying(50) NOT NULL,
    ""Status"" character varying(20) NOT NULL,
    ""ExpiresAt"" timestamp with time zone NOT NULL,
    ""CreatedAt"" timestamp with time zone NOT NULL,
    ""UpdatedAt"" timestamp with time zone,
    ""ConfirmedAt"" timestamp with time zone,
    CONSTRAINT ""FK_AdmissionHolds_AdmissionClasses_ClassId"" FOREIGN KEY (""ClassId"") REFERENCES ""AdmissionClasses"" (""Id"") ON DELETE CASCADE
);
CREATE INDEX IF NOT EXISTS ""IX_AdmissionHolds_ClassId_Status"" ON ""AdmissionHolds"" (""ClassId"", ""Status"");
CREATE INDEX IF NOT EXISTS ""IX_AdmissionHolds_ExpiresAt_Status"" ON ""AdmissionHolds"" (""ExpiresAt"", ""Status"");
CREATE INDEX IF NOT EXISTS ""IX_AdmissionHolds_StudentId"" ON ""AdmissionHolds"" (""StudentId"");
CREATE UNIQUE INDEX IF NOT EXISTS ""IX_AdmissionHolds_StudentId_ClassId_Active"" ON ""AdmissionHolds"" (""StudentId"", ""ClassId"") WHERE ""Status"" IN ('HELD','CONFIRMED');
");
}
