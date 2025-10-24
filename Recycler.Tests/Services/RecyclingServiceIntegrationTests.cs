using Dapper;
using Npgsql;
using Recycler.API.Models;
using Recycler.API.Services;
using Recycler.Tests.Infrastructure;
using Xunit;

namespace Recycler.Tests.Services;

public class RecyclingServiceIntegrationTests : IClassFixture<PostgreSqlFixture>, IAsyncLifetime
{
    private readonly PostgreSqlFixture _fixture;
    private readonly IRecyclingService _recyclingService;

    public RecyclingServiceIntegrationTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
        var connectionString = _fixture.Container!.GetConnectionString();
        _recyclingService = new TestRecyclingService(connectionString);
    }

    public async Task InitializeAsync()
    {
        await _fixture.TruncateTables();
        
        await using var connection = new NpgsqlConnection(_fixture.Container!.GetConnectionString());
        await connection.OpenAsync();

        await connection.ExecuteAsync("INSERT INTO AuditActions (id, action_name) VALUES (1, 'INSERT'), (2, 'UPDATE')");

        await connection.ExecuteAsync("INSERT INTO Role (id, name) VALUES (1, 'Recycler'), (2, 'Supplier'), (3, 'Logistics'), (4, 'Bank')");

        await connection.ExecuteAsync(@"
            INSERT INTO Companies (id, role_id, name, key_id) 
            VALUES (1, 1, 'TestCo', 1), (2, 2, 'TestSup', 2)");

        await connection.ExecuteAsync(@"
            INSERT INTO PhoneBrand (id, brand_name) 
            VALUES (1, 'TestBrand')");

        await connection.ExecuteAsync(@"
            INSERT INTO Phone (id, model, phone_brand_id) 
            VALUES (1, 'TestModel1', 1), (2, 'TestModel2', 1)");

        await connection.ExecuteAsync(@"
            INSERT INTO PhoneInventory (id, phone_id, quantity) 
            VALUES (1, 1, 10), (2, 2, 5)");

        await connection.ExecuteAsync(@"
            INSERT INTO RawMaterial (id, name, price_per_kg) 
            VALUES (1, 'Gold', 50.00), (2, 'Silver', 25.00), (3, 'Copper', 5.00)");

        await connection.ExecuteAsync(@"
            INSERT INTO PhoneParts (id, name) 
            VALUES (1, 'Screen'), (2, 'Battery'), (3, 'Circuit Board')");

        await connection.ExecuteAsync(@"
            INSERT INTO PhoneToPhonePartRatio (phone_id, phone_part_id, phone_part_quantity_per_phone) 
            VALUES (1, 1, 1), (1, 2, 1), (1, 3, 1), (2, 1, 1), (2, 2, 1), (2, 3, 1)");

        await connection.ExecuteAsync(@"
            INSERT INTO PhonePartToRawMaterialRatio (phone_part_id, raw_material_id, raw_material_quantity_per_phone_part) 
            VALUES (1, 1, 0.1), (2, 2, 0.05), (3, 3, 0.2)");

        await connection.ExecuteAsync(@"
            INSERT INTO Machines (id, machine_id, is_operational) 
            VALUES (1, 1, true), (2, 2, true)");
    }

    public Task DisposeAsync() => Task.CompletedTask;





}
