using Dapper;
using Npgsql;
using Recycler.API.Models;
using Recycler.API.Services;
using Recycler.API;
using Recycler.Tests.Infrastructure;
using Xunit;

namespace Recycler.Tests.Services;

public class RawMaterialServiceIntegrationTests : IClassFixture<PostgreSqlFixture>, IAsyncLifetime
{
    private readonly PostgreSqlFixture _fixture;
    private readonly RawMaterialService _rawMaterialService;
    private readonly IGenericRepository<RawMaterial> _rawMaterialRepository;
    private readonly IGenericRepository<MaterialInventory> _materialInventoryRepository;

    public RawMaterialServiceIntegrationTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
        var connectionString = _fixture.Container!.GetConnectionString();
        _rawMaterialRepository = new TestGenericRepository<RawMaterial>(connectionString);
        _materialInventoryRepository = new TestGenericRepository<MaterialInventory>(connectionString);
        _rawMaterialService = new RawMaterialService(_rawMaterialRepository, _materialInventoryRepository);
    }

    public async Task InitializeAsync()
    {
        await _fixture.TruncateTables();
        
        await using var connection = new NpgsqlConnection(_fixture.Container!.GetConnectionString());
        await connection.OpenAsync();

        await connection.ExecuteAsync(@"
            INSERT INTO AuditActions (id, action_name) 
            VALUES (1, 'INSERT'), (2, 'UPDATE')");

        await connection.ExecuteAsync(@"
            INSERT INTO RawMaterial (id, name, price_per_kg) 
            VALUES (1, 'Gold', 50.00), (2, 'Silver', 25.00), (3, 'Copper', 5.00)");

        await connection.ExecuteAsync(@"
            INSERT INTO MaterialInventory (id, material_id, available_quantity_in_kg, reserved_quantity_in_kg) 
            VALUES (1, 1, 100.0, 0.0), (2, 2, 50.0, 10.0), (3, 3, 200.0, 0.0)");
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllRawMaterials()
    {
        var materials = await _rawMaterialService.GetAllAsync();

        Assert.NotNull(materials);
        var materialsList = materials.ToList();
        Assert.Equal(3, materialsList.Count);
        Assert.Contains(materialsList, m => m.Name == "Gold");
        Assert.Contains(materialsList, m => m.Name == "Silver");
        Assert.Contains(materialsList, m => m.Name == "Copper");
    }


    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenMaterialNotFound()
    {
        var material = await _rawMaterialService.GetByIdAsync(999);

        Assert.Null(material);
    }




}
