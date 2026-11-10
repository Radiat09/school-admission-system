using Microsoft.EntityFrameworkCore;
using SchoolAdmission.Api.Entities;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using SchoolAdmission.Api.DTOs;
using Xunit;

namespace SchoolAdmission.Tests;

[Collection("SharedDb")]
public class ConcurrencyTests : IAsyncLifetime
{
    private readonly AdmissionApiFactory _factory;
    private readonly SharedDbFixture _fixture;
    private HttpClient _client = null!;
    private string _classId = null!;

    public ConcurrencyTests(SharedDbFixture fixture)
    {
        _fixture = fixture;
        _factory = new AdmissionApiFactory(fixture);
    }

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();
        await _fixture.Truncate();

        var createResp = await _client.PostAsJsonAsync("/classes", new { name = "Class 10 - Morning", totalSeats = 10 });
        createResp.EnsureSuccessStatusCode();
        var created = await createResp.Content.ReadFromJsonAsync<ClassDto>();
        _classId = created!.Id;
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Hundred_Concurrent_Applications_For_Ten_Seats_Do_Not_Oversell()
    {
        const int seats = 10;
        const int applicants = 100;

        var tasks = Enumerable.Range(0, applicants).Select(async i =>
        {
            var req = new HttpRequestMessage(HttpMethod.Post, $"/classes/{_classId}/holds");
            req.Headers.Add("X-Student-Id", StudentIdGenerator.Next());
            var resp = await _client.SendAsync(req);
            var body = await resp.Content.ReadAsStringAsync();
            req.Dispose();
            return (Status: resp.StatusCode, Body: body);
        });

        var results = await Task.WhenAll(tasks);

        var success = results.Count(s => s.Status == HttpStatusCode.Created);
        var conflict = results.Count(s => s.Status == HttpStatusCode.Conflict);
        var unexpected = results.Where(s => s.Status != HttpStatusCode.Created && s.Status != HttpStatusCode.Conflict).ToList();

        if (unexpected.Any())
        {
            Console.WriteLine("UNEXPECTED: " + string.Join(" | ", unexpected.Select(s => $"{s.Status}:{s.Body}")));
        }

        Assert.Empty(unexpected);
        Assert.Equal(seats, success);
        Assert.Equal(applicants, success + conflict);
        Assert.Equal(applicants - seats, conflict);

        // Verify database state
        using var ctx = _fixture.CreateContext();
        var heldCount = await ctx.AdmissionHolds.CountAsync(h => h.ClassId == _classId && h.Status == HoldStatus.HELD);
        var confirmedCount = await ctx.AdmissionHolds.CountAsync(h => h.ClassId == _classId && h.Status == HoldStatus.CONFIRMED);

        Assert.Equal(seats, heldCount);
        Assert.Equal(0, confirmedCount);
        Assert.Equal(seats, heldCount + confirmedCount);

        // Class endpoint reports zero available
        var classResp = await _client.GetAsync($"/classes/{_classId}");
        classResp.EnsureSuccessStatusCode();
        var classDto = await classResp.Content.ReadFromJsonAsync<ClassDto>();
        Assert.Equal(0, classDto!.AvailableSeats);
        Assert.Equal(seats, classDto.HeldSeats);
    }
}
