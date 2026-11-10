using System.Net;
using System.Net.Http.Json;
using SchoolAdmission.Api.DTOs;
using SchoolAdmission.Api.Entities;
using Xunit;

namespace SchoolAdmission.Tests;

[Collection("SharedDb")]
public class AdmissionFlowTests : IAsyncLifetime
{
    private readonly AdmissionApiFactory _factory;
    private readonly SharedDbFixture _fixture;
    private HttpClient _client = null!;
    private string _classId = null!;

    public AdmissionFlowTests(SharedDbFixture fixture)
    {
        _fixture = fixture;
        _factory = new AdmissionApiFactory(fixture);
    }

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Student-Id", StudentIdGenerator.Next());
        await _fixture.Truncate();

        var createResp = await _client.PostAsJsonAsync("/classes", new { name = "Class 6 - Morning Shift", totalSeats = 120 });
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
    public async Task Normal_Admission_Flow_Creates_Hold_And_Confirms_Once()
    {
        // Apply
        var applyResp = await _client.PostAsync($"/classes/{_classId}/holds", null);
        Assert.Equal(HttpStatusCode.Created, applyResp.StatusCode);
        var hold = await applyResp.Content.ReadFromJsonAsync<CreateHoldResponse>();
        Assert.NotNull(hold);
        Assert.Equal("HELD", hold.Status);
        Assert.Equal(_classId, hold.ClassId);
        Assert.True(hold.ExpiresAt > DateTime.UtcNow);

        // Get hold details
        var getResp = await _client.GetAsync($"/holds/{hold.HoldId}");
        Assert.Equal(HttpStatusCode.OK, getResp.StatusCode);
        var holdDto = await getResp.Content.ReadFromJsonAsync<HoldDto>();
        Assert.Equal("HELD", holdDto!.Status);

        // Class counts: held=1, available=119
        var classResp = await _client.GetAsync($"/classes/{_classId}");
        Assert.Equal(HttpStatusCode.OK, classResp.StatusCode);
        var classDto = await classResp.Content.ReadFromJsonAsync<ClassDto>();
        Assert.Equal(1, classDto!.HeldSeats);
        Assert.Equal(0, classDto.ConfirmedSeats);
        Assert.Equal(119, classDto.AvailableSeats);

        // Confirm
        _client.DefaultRequestHeaders.Remove("X-Student-Id");
        _client.DefaultRequestHeaders.Add("X-Student-Id", hold.StudentId);
        var confirmResp = await _client.PostAsync($"/holds/{hold.HoldId}/confirm", null);
        Assert.Equal(HttpStatusCode.OK, confirmResp.StatusCode);
        var confirmDto = await confirmResp.Content.ReadFromJsonAsync<ConfirmResponse>();
        Assert.Equal("CONFIRMED", confirmDto!.Status);

        // Idempotent confirm again
        var confirmResp2 = await _client.PostAsync($"/holds/{hold.HoldId}/confirm", null);
        Assert.Equal(HttpStatusCode.OK, confirmResp2.StatusCode);

        // Confirmed seats increased exactly once
        var classResp2 = await _client.GetAsync($"/classes/{_classId}");
        var classDto2 = await classResp2.Content.ReadFromJsonAsync<ClassDto>();
        Assert.Equal(0, classDto2!.HeldSeats);
        Assert.Equal(1, classDto2.ConfirmedSeats);
        Assert.Equal(119, classDto2.AvailableSeats);

        // Confirm expired? not expired here. Verify hold status confirmed.
        var getResp2 = await _client.GetAsync($"/holds/{hold.HoldId}");
        var holdDto2 = await getResp2.Content.ReadFromJsonAsync<HoldDto>();
        Assert.Equal("CONFIRMED", holdDto2!.Status);
    }

    [Fact]
    public async Task Missing_Student_Header_Returns_400()
    {
        var client = _factory.CreateClient();
        var resp = await client.PostAsync($"/classes/{_classId}/holds", null);
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task Invalid_Student_Id_Returns_400()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Student-Id", "not-a-valid-id");
        var resp = await client.PostAsync($"/classes/{_classId}/holds", null);
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task Missing_Class_Returns_404()
    {
        var resp = await _client.GetAsync("/classes/nope");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task Duplicate_Active_Application_Returns_409()
    {
        var applyResp = await _client.PostAsync($"/classes/{_classId}/holds", null);
        Assert.Equal(HttpStatusCode.Created, applyResp.StatusCode);
        var hold = await applyResp.Content.ReadFromJsonAsync<CreateHoldResponse>();

        var client2 = _factory.CreateClient();
        client2.DefaultRequestHeaders.Add("X-Student-Id", hold!.StudentId);
        var dupResp = await client2.PostAsync($"/classes/{_classId}/holds", null);
        Assert.Equal(HttpStatusCode.Conflict, dupResp.StatusCode);
    }

    [Fact]
    public async Task Cancellation_Releases_Seat_And_Allows_Reapply()
    {
        var applyResp = await _client.PostAsync($"/classes/{_classId}/holds", null);
        Assert.Equal(HttpStatusCode.Created, applyResp.StatusCode);
        var hold = await applyResp.Content.ReadFromJsonAsync<CreateHoldResponse>();

        var client2 = _factory.CreateClient();
        client2.DefaultRequestHeaders.Add("X-Student-Id", hold!.StudentId);
        var cancelResp = await client2.DeleteAsync($"/holds/{hold.HoldId}");
        Assert.Equal(HttpStatusCode.NoContent, cancelResp.StatusCode);

        // Cannot cancel again
        var cancelResp2 = await client2.DeleteAsync($"/holds/{hold.HoldId}");
        Assert.Equal(HttpStatusCode.Conflict, cancelResp2.StatusCode);

        // Cannot confirm a cancelled hold
        var confirmResp = await client2.PostAsync($"/holds/{hold.HoldId}/confirm", null);
        Assert.Equal(HttpStatusCode.Conflict, confirmResp.StatusCode);

        // Reapply after cancellation succeeds
        var reapplyResp = await client2.PostAsync($"/classes/{_classId}/holds", null);
        Assert.Equal(HttpStatusCode.Created, reapplyResp.StatusCode);
    }
}
