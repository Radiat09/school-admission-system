using System.Net;
using System.Net.Http.Json;
using SchoolAdmission.Api.DTOs;
using Xunit;

namespace SchoolAdmission.Tests;

[Collection("SharedDb")]
public class AdditionalBehaviorTests : IAsyncLifetime
{
    private readonly AdmissionApiFactory _factory;
    private readonly SharedDbFixture _fixture;
    private HttpClient _client = null!;
    private string _classId = null!;

    public AdditionalBehaviorTests(SharedDbFixture fixture)
    {
        _fixture = fixture;
        _factory = new AdmissionApiFactory(fixture);
    }

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Student-Id", StudentIdGenerator.Next());
        await _fixture.Truncate();

        var createResp = await _client.PostAsJsonAsync("/classes", new { name = "Class 7 - Day", totalSeats = 3 });
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
    public async Task Cancelled_Hold_Cannot_Be_Confirmed()
    {
        var hold = await ApplyAndGetHold();
        _client.DefaultRequestHeaders.Remove("X-Student-Id");
        _client.DefaultRequestHeaders.Add("X-Student-Id", hold.StudentId);

        var cancelResp = await _client.DeleteAsync($"/holds/{hold.HoldId}");
        Assert.Equal(HttpStatusCode.NoContent, cancelResp.StatusCode);

        // Confirming a cancelled hold is rejected
        var confirmResp = await _client.PostAsync($"/holds/{hold.HoldId}/confirm", null);
        Assert.Equal(HttpStatusCode.Conflict, confirmResp.StatusCode);

        // Reapply after cancellation succeeds
        var reapplyResp = await _client.PostAsync($"/classes/{_classId}/holds", null);
        Assert.Equal(HttpStatusCode.Created, reapplyResp.StatusCode);
    }

    [Fact]
    public async Task Get_My_Holds_Returns_Own_Applications()
    {
        var hold1 = await ApplyAndGetHold();

        // A different student applies to the same class
        var otherClient = _factory.CreateClient();
        otherClient.DefaultRequestHeaders.Add("X-Student-Id", StudentIdGenerator.Next());
        var otherHold = await otherClient.PostAsync($"/classes/{_classId}/holds", null);
        Assert.Equal(HttpStatusCode.Created, otherHold.StatusCode);
        otherClient.Dispose();

        var meResp = await _client.GetAsync("/me/holds");
        Assert.Equal(HttpStatusCode.OK, meResp.StatusCode);
        var holds = await meResp.Content.ReadFromJsonAsync<List<MyHoldDto>>();
        Assert.Single(holds!);
        Assert.Equal(hold1.HoldId, holds[0].HoldId);
    }

    [Fact]
    public async Task Get_All_Classes_Returns_Seat_Counts()
    {
        var listResp = await _client.GetAsync("/classes");
        Assert.Equal(HttpStatusCode.OK, listResp.StatusCode);
        var list = await listResp.Content.ReadFromJsonAsync<List<ClassDto>>();
        var c = list!.First(x => x.Id == _classId);
        Assert.Equal(3, c.TotalSeats);
        Assert.Equal(0, c.HeldSeats);
        Assert.Equal(0, c.ConfirmedSeats);
        Assert.Equal(3, c.AvailableSeats);
    }

    private async Task<CreateHoldResponse> ApplyAndGetHold()
    {
        var resp = await _client.PostAsync($"/classes/{_classId}/holds", null);
        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
        return await resp.Content.ReadFromJsonAsync<CreateHoldResponse>()!;
    }
}
