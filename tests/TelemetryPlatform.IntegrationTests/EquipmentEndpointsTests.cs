using System.Net;
using System.Net.Http.Json;
using Shouldly;
using TelemetryPlatform.Application.Equipments;

namespace TelemetryPlatform.IntegrationTests;

public sealed class EquipmentEndpointsTests(TelemetryApiFactory factory) : IClassFixture<TelemetryApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    // O banco é compartilhado entre os testes da classe: cada teste usa uma tag única.
    private static string UniqueTag() => $"T-{Guid.NewGuid():N}"[..20];

    [Fact]
    public async Task Post_ThenGetById_ReturnsCreatedEquipment()
    {
        var request = new CreateEquipmentRequest(UniqueTag(), "Termopar cadinho", "Alto Forno 1");

        var post = await _client.PostAsJsonAsync("/api/equipments", request, Ct);

        post.StatusCode.ShouldBe(HttpStatusCode.Created);
        post.Headers.Location.ShouldNotBeNull();

        var created = await post.Content.ReadFromJsonAsync<EquipmentResponse>(Ct);
        var fetched = await _client.GetFromJsonAsync<EquipmentResponse>(post.Headers.Location, Ct);

        fetched.ShouldBe(created);
    }

    [Fact]
    public async Task Post_WithDuplicateTag_Returns409()
    {
        var request = new CreateEquipmentRequest(UniqueTag(), "Termopar", "Alto Forno 1");
        (await _client.PostAsJsonAsync("/api/equipments", request, Ct)).EnsureSuccessStatusCode();

        var duplicate = await _client.PostAsJsonAsync("/api/equipments", request, Ct);

        duplicate.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Post_WithInvalidTag_Returns400ProblemDetails()
    {
        var request = new CreateEquipmentRequest("tag inválida!", "Termopar", "Alto Forno 1");

        var response = await _client.PostAsJsonAsync("/api/equipments", request, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
    }

    [Fact]
    public async Task GetById_WhenMissing_Returns404()
    {
        var response = await _client.GetAsync($"/api/equipments/{Guid.NewGuid()}", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Health_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/health", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
