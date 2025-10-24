using Dapper;
using Npgsql;
using Recycler.API.Models;
using Recycler.API.Services;
using Recycler.API;
using Recycler.Tests.Infrastructure;
using Xunit;

namespace Recycler.Tests.Services;

public class LogServiceIntegrationTests : IClassFixture<PostgreSqlFixture>, IAsyncLifetime
{
    private readonly PostgreSqlFixture _fixture;
    private readonly LogService _logService;
    private readonly IGenericRepository<Log> _logRepository;

    public LogServiceIntegrationTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
        var connectionString = _fixture.Container!.GetConnectionString();
        Console.WriteLine($"Connection string: {connectionString}");
        
        _logRepository = new TestGenericRepository<Log>(connectionString);
        _logService = new LogService(_logRepository);
    }

    public async Task InitializeAsync()
    {
        await _fixture.TruncateTables();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllLogs()
    {
        await using var connection = new NpgsqlConnection(_fixture.Container!.GetConnectionString());
        await connection.OpenAsync();

        await connection.ExecuteAsync(@"
            INSERT INTO Log (request_source, request_endpoint, request_body, response, timestamp) 
            VALUES 
                ('TestSource1', '/api/test1', '{}', '{}', NOW()),
                ('TestSource2', '/api/test2', '{}', '{}', NOW())");

        var logs = await _logService.GetAllAsync();

        Assert.NotNull(logs);
        var logsList = logs.ToList();
        Assert.Equal(2, logsList.Count);
    }


    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenLogNotFound()
    {
        var log = await _logService.GetByIdAsync(999);

        Assert.Null(log);
    }


    [Fact]
    public async Task GetLogs_ShouldReturnAllLogs_WhenMaxIdIsNull()
    {
        await using var connection = new NpgsqlConnection(_fixture.Container!.GetConnectionString());
        await connection.OpenAsync();

        await connection.ExecuteAsync(@"
            INSERT INTO Log (request_source, request_endpoint, request_body, response, timestamp) 
            VALUES 
                ('Source1', '/api/test1', '{}', '{}', NOW()),
                ('Source2', '/api/test2', '{}', '{}', NOW())");

        var logs = await _logService.GetLogs(null);

        Assert.NotNull(logs);
        var logsList = logs.ToList();
        Assert.Equal(2, logsList.Count);
    }



}
