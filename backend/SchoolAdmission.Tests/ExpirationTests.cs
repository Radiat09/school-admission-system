using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SchoolAdmission.Api;
using SchoolAdmission.Api.Data;
using SchoolAdmission.Api.Entities;
using SchoolAdmission.Api.Repositories;
using SchoolAdmission.Api.Services;
using Xunit;

namespace SchoolAdmission.Tests;

[Collection("SharedDb")]
public class ExpirationTests : IAsyncLifetime
{
    private readonly SharedDbFixture _fixture;
    private readonly TestTimeProvider _timeProvider;
    private IAdmissionService _service = null!;

    public ExpirationTests(SharedDbFixture fixture)
    {
        _fixture = fixture;
        _timeProvider = new TestTimeProvider(new DateTime(2026, 11, 1, 4, 0, 0, DateTimeKind.Utc));
    }

    public async Task InitializeAsync()
    {
        await _fixture.Truncate();

        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(o => o.UseNpgsql(_fixture.ConnectionString));
        services.AddScoped<IClassRepository, ClassRepository>();
        services.AddScoped<IHoldRepository, HoldRepository>();
        services.AddSingleton<TimeProvider>(_timeProvider);
        services.AddTransient<IAdmissionService>(sp => new AdmissionService(
            sp.GetRequiredService<AppDbContext>(),
            sp.GetRequiredService<IClassRepository>(),
            sp.GetRequiredService<IHoldRepository>(),
            _timeProvider,
            holdDurationSeconds: 600));

        var sp = services.BuildServiceProvider();
        _service = sp.GetRequiredService<IAdmissionService>();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Expired_Hold_Releases_Seat_And_Rejects_Confirmation()
    {
        var classDto = await _service.CreateClassAsync("Class 5 - Morning", 5);
        var hold = await _service.ApplyAsync(classDto.Id, StudentIdGenerator.Next());

        Assert.Equal("HELD", hold.Status);
        Assert.Equal(_timeProvider.GetUtcNow().UtcDateTime.AddSeconds(600), hold.ExpiresAt);

        // Advance beyond expiration
        _timeProvider.Advance(TimeSpan.FromSeconds(601));

        var expired = await _service.ReleaseExpiredHoldsAsync();
        Assert.Equal(1, expired);

        var holdDto = await _service.GetHoldAsync(hold.HoldId);
        Assert.Equal("EXPIRED", holdDto.Status);

        // Seat available again
        var classDto2 = await _service.GetClassAsync(classDto.Id);
        Assert.Equal(0, classDto2!.HeldSeats);
        Assert.Equal(0, classDto2.ConfirmedSeats);
        Assert.Equal(5, classDto2.AvailableSeats);

        // Confirming expired hold is rejected
        var ex = await Assert.ThrowsAsync<ConflictException>(() => _service.ConfirmAsync(hold.HoldId, hold.StudentId));
        Assert.Contains("expired", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
