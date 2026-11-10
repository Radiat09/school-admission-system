using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SchoolAdmission.Api;
using SchoolAdmission.Api.Data;
using SchoolAdmission.Api.Repositories;
using SchoolAdmission.Api.Services;
using Xunit;

namespace SchoolAdmission.Tests;

public class SharedDbFixture : IAsyncLifetime
{
    public string ConnectionString { get; } =
        Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
        ?? "Host=localhost;Port=5432;Database=schooladmission;Username=postgres;Password=postgres;MaxPoolSize=200;";

    public AppDbContext CreateContext()
    {
        var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        return new AppDbContext(options);
    }

    public async Task InitializeAsync()
    {
        using var ctx = CreateContext();
        ctx.Database.Migrate();
        await TruncateAsync(ctx);
    }

    public async Task DisposeAsync() { }

    public async Task Truncate()
    {
        using var ctx = CreateContext();
        await TruncateAsync(ctx);
    }

    private static async Task TruncateAsync(AppDbContext ctx)
    {
        await ctx.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"AdmissionHolds\", \"AdmissionClasses\" CASCADE;");
    }
}

[CollectionDefinition("SharedDb")]
public class SharedDbCollection : ICollectionFixture<SharedDbFixture> { }

public class AdmissionApiFactory : WebApplicationFactory<Program>
{
    private readonly SharedDbFixture _fixture;
    public AdmissionApiFactory(SharedDbFixture fixture) => _fixture = fixture;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new System.Collections.Generic.KeyValuePair<string, string>[]
            {
                new("ConnectionStrings__DefaultConnection", _fixture.ConnectionString),
                new("CORS_ALLOWED_ORIGINS", "http://localhost:3000"),
                new("HOLD_DURATION_SECONDS", "600"),
            });
        });
    }
}

public class TestTimeProvider : TimeProvider
{
    private DateTime _now;
    public TestTimeProvider(DateTime start) => _now = DateTime.SpecifyKind(start, DateTimeKind.Utc);
    public override DateTimeOffset GetUtcNow() => new DateTimeOffset(_now, TimeSpan.Zero);
    public void Advance(TimeSpan span) => _now = _now.Add(span);
    public void Set(DateTime utcNow) => _now = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);
}

public static class StudentIdGenerator
{
    private static int _counter;
    public static string Next()
    {
        var value = System.Threading.Interlocked.Increment(ref _counter);
        return $"2015269{value:D10}"; // 7 + 10 = 17 digits
    }
}
