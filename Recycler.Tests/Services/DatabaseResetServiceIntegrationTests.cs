using Dapper;
using Npgsql;
using Recycler.API.Services;
using Recycler.Tests.Infrastructure;
using Xunit;

namespace Recycler.Tests.Services;

public class DatabaseResetServiceIntegrationTests : IClassFixture<PostgreSqlFixture>, IAsyncLifetime
{
    private readonly PostgreSqlFixture _fixture;
    private readonly IDatabaseResetService _databaseResetService;

    public DatabaseResetServiceIntegrationTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
        var connectionString = _fixture.Container!.GetConnectionString();
        _databaseResetService = new TestDatabaseResetService(connectionString);
    }

    public async Task InitializeAsync()
    {
        await _fixture.TruncateTables();
        
        await using var connection = new NpgsqlConnection(_fixture.Container!.GetConnectionString());
        await connection.OpenAsync();

        await connection.ExecuteAsync("INSERT INTO AuditActions (id, action_name) VALUES (1, 'INSERT'), (2, 'UPDATE')");

        await connection.ExecuteAsync("INSERT INTO Role (id, name) VALUES (1, 'Test Role')");

        await connection.ExecuteAsync(@"
            INSERT INTO Companies (id, role_id, name, key_id) 
            VALUES (1, 1, 'TestCo', 1)");

        await connection.ExecuteAsync(@"
            INSERT INTO RawMaterial (id, name, price_per_kg) 
            VALUES (1, 'Copper', 10.00)");

        await connection.ExecuteAsync(@"
            INSERT INTO MaterialInventory (id, material_id, available_quantity_in_kg, reserved_quantity_in_kg) 
            VALUES (1, 1, 100.0, 0.0)");

        await connection.ExecuteAsync(@"
            INSERT INTO Log (id, request_source, request_endpoint, request_body, response, timestamp) 
            VALUES (1, 'TestSource', '/api/test', '{}', '{}', NOW())");
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task ResetAsync_ShouldClearAllTables()
    {
        await using var connection = new NpgsqlConnection(_fixture.Container!.GetConnectionString());
        
        var roleCount = await connection.QuerySingleAsync<int>("SELECT COUNT(*) FROM Role");
        var companyCount = await connection.QuerySingleAsync<int>("SELECT COUNT(*) FROM Companies");
        var materialCount = await connection.QuerySingleAsync<int>("SELECT COUNT(*) FROM RawMaterial");
        var inventoryCount = await connection.QuerySingleAsync<int>("SELECT COUNT(*) FROM MaterialInventory");
        var logCount = await connection.QuerySingleAsync<int>("SELECT COUNT(*) FROM Log");

        Assert.True(roleCount > 0);
        Assert.True(companyCount > 0);
        Assert.True(materialCount > 0);
        Assert.True(inventoryCount > 0);
        Assert.True(logCount > 0);

        await _databaseResetService.ResetAsync(CancellationToken.None);

        roleCount = await connection.QuerySingleAsync<int>("SELECT COUNT(*) FROM Role");
        companyCount = await connection.QuerySingleAsync<int>("SELECT COUNT(*) FROM Companies");
        materialCount = await connection.QuerySingleAsync<int>("SELECT COUNT(*) FROM RawMaterial");
        inventoryCount = await connection.QuerySingleAsync<int>("SELECT COUNT(*) FROM MaterialInventory");
        logCount = await connection.QuerySingleAsync<int>("SELECT COUNT(*) FROM Log");

        Assert.Equal(0, roleCount);
        Assert.Equal(0, companyCount);
        Assert.Equal(0, materialCount);
        Assert.Equal(0, inventoryCount);
        Assert.Equal(0, logCount);
    }


    [Fact]
    public async Task ResetAsync_ShouldHandleEmptyDatabase()
    {
        await _databaseResetService.ResetAsync(CancellationToken.None);

        await _databaseResetService.ResetAsync(CancellationToken.None);

        await using var connection = new NpgsqlConnection(_fixture.Container!.GetConnectionString());
        var roleCount = await connection.QuerySingleAsync<int>("SELECT COUNT(*) FROM Role");
        Assert.Equal(0, roleCount);
    }

    [Fact]
    public async Task ResetAsync_ShouldCompleteSuccessfully()
    {
        var exception = await Record.ExceptionAsync(() => 
            _databaseResetService.ResetAsync(CancellationToken.None));
        
        Assert.Null(exception);
    }

    [Fact]
    public async Task ResetAsync_ShouldResetAllRelatedTables()
    {
        await using var connection = new NpgsqlConnection(_fixture.Container!.GetConnectionString());
        await connection.OpenAsync();

        await connection.ExecuteAsync(@"
            INSERT INTO OrderStatus (id, name) VALUES (1, 'Pending')");

        await connection.ExecuteAsync(@"
            INSERT INTO Orders (id, order_number, order_status_id, created_at, company_id, order_expires_at) 
            VALUES (1, '12345678-1234-1234-1234-123456789012', 1, NOW(), 1, NOW() + INTERVAL '1 day')");

        await connection.ExecuteAsync(@"
            INSERT INTO OrderItems (id, order_id, material_id, quantity_in_kg, price_per_kg) 
            VALUES (1, 1, 1, 10.0, 10.00)");

        var orderCount = await connection.QuerySingleAsync<int>("SELECT COUNT(*) FROM Orders");
        var orderItemCount = await connection.QuerySingleAsync<int>("SELECT COUNT(*) FROM OrderItems");
        Assert.True(orderCount > 0);
        Assert.True(orderItemCount > 0);

        await _databaseResetService.ResetAsync(CancellationToken.None);

        orderCount = await connection.QuerySingleAsync<int>("SELECT COUNT(*) FROM Orders");
        orderItemCount = await connection.QuerySingleAsync<int>("SELECT COUNT(*) FROM OrderItems");
        var orderStatusCount = await connection.QuerySingleAsync<int>("SELECT COUNT(*) FROM OrderStatus");

        Assert.Equal(0, orderCount);
        Assert.Equal(0, orderItemCount);
        Assert.Equal(0, orderStatusCount);
    }
}
