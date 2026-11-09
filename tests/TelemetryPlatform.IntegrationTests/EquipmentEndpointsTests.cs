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
    public async Task Delete_CreateAndDelete_Returns204()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/equipments", new CreateEquipmentRequest(UniqueTag(), "Termopar cadinho", "Alto Forno 1"), Ct);
        createResponse.EnsureSuccessStatusCode();
        EquipmentResponse? created = await createResponse.Content.ReadFromJsonAsync<EquipmentResponse>(Ct);
    
        var equipmentId = created?.Id ?? throw new InvalidOperationException("Failed to read created equipment ID.");

        var deleteResponse = await _client.DeleteAsync($"/api/equipments/{equipmentId}", Ct);
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/equipments/{equipmentId}", Ct);
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    
    [Fact]
    public async Task Delete_WhenEquipmentDoesNotExist_Returns404()
    {
        var response = await _client.DeleteAsync($"/api/equipments/{Guid.NewGuid()}", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_CreateThenUpdate_ReturnsUpdatedEquipment()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/equipments", new CreateEquipmentRequest(UniqueTag(), "Termopar cadinho", "Alto Forno 1"), Ct);
        createResponse.EnsureSuccessStatusCode();

        EquipmentResponse? created = await createResponse.Content.ReadFromJsonAsync<EquipmentResponse>(Ct);

        var equipmentId = created?.Id ?? throw new InvalidOperationException("Failed to read created equipment ID.");

        var request = new UpdateEquipmentRequest("Termopar atualizado", "Alto Forno 2");

        var updateResponse = await _client.PutAsJsonAsync($"/api/equipments/{equipmentId}", request, Ct);
        updateResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        EquipmentResponse? updated = await updateResponse.Content.ReadFromJsonAsync<EquipmentResponse>(Ct);
        updated.ShouldNotBeNull();
        updated.Name.ShouldBe("Termopar atualizado");
        updated.Area.ShouldBe("Alto Forno 2");
        updated.Tag.ShouldBe(created.Tag); // Tag não deve mudar
        updated.Id.ShouldBe(equipmentId); // Id não deve mudar

        var fetched = await _client.GetFromJsonAsync<EquipmentResponse>($"/api/equipments/{equipmentId}", Ct);
        fetched.ShouldBe(updated);
    }



    [Fact]
    public async Task Health_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/health", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
