using Dapper;
using Npgsql;

public interface IDatabaseResetService
{
    Task ResetAsync(CancellationToken cancellationToken);
}

public class DatabaseResetService : IDatabaseResetService
{
    private readonly IConfiguration _configuration;

    public DatabaseResetService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task ResetAsync(CancellationToken cancellationToken)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");

        using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var resetSql = @"
            UPDATE MaterialInventory SET available_quantity_in_kg = 0, reserved_quantity_in_kg = 0;
            UPDATE PhoneInventory SET quantity = 0;
            DELETE FROM OrderItems;
            DELETE FROM Orders;
            DELETE FROM Machines;
        ";

        await connection.ExecuteAsync(resetSql);
    }
}
