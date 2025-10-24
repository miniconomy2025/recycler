using Testcontainers.PostgreSql;
using Npgsql;
using Dapper;

namespace Recycler.Tests.Infrastructure;

public class PostgreSqlFixture : IAsyncLifetime
{
    public PostgreSqlContainer? Container { get; private set; }
    public NpgsqlConnection Connection => new(Container?.GetConnectionString());

    public async Task InitializeAsync()
    {
        Container = new PostgreSqlBuilder()
            .WithDatabase("testdb")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .Build();

        await Container.StartAsync();

        var connectionString = Container.GetConnectionString();

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        var migrationPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "Recycler.API", "dbMigrations", "V1__init.sql");
        if (File.Exists(migrationPath))
        {
            var setupSql = await File.ReadAllTextAsync(migrationPath);
            await conn.ExecuteAsync(setupSql);
        }

        await ApplyMigration(conn, "V2__add_audit_actions.sql");
        await ApplyMigration(conn, "V3__add_raw_material_audit_table.sql");
        await ApplyMigration(conn, "V4__add_phone_inventory_audit_table.sql");
        await ApplyMigration(conn, "V5__add_material_inventory_audit_table.sql");
        await ApplyMigration(conn, "V6__add_orders_audit_table.sql");
        await ApplyMigration(conn, "V7__add_phone_to_phone_part_ratio_audit_table.sql");
        await ApplyMigration(conn, "V8__add_phone_part_to_raw_material_ratio_audit_table.sql");
        await ApplyMigration(conn, "V9__add_machines_audit_table.sql");
        await ApplyMigration(conn, "V10__seed.sql");
        await ApplyMigration(conn, "V11__dummy_insert_data.sql");
        await ApplyMigration(conn, "V12__add_logs_table.sql");
    }

    private async Task ApplyMigration(NpgsqlConnection conn, string migrationFile)
    {
        var migrationPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "Recycler.API", "dbMigrations", migrationFile);
        if (File.Exists(migrationPath))
        {
            var migrationSql = await File.ReadAllTextAsync(migrationPath);
            await conn.ExecuteAsync(migrationSql);
        }
    }

    public TestDbConnectionFactory ConnectionFactory => new(Container!.GetConnectionString());

    public async Task TruncateTables()
    {
        await using var conn = new NpgsqlConnection(Container!.GetConnectionString());
        await conn.ExecuteAsync(@"
            TRUNCATE TABLE 
                Log, 
                OrdersAuditLogs, 
                PhonePartToRawMaterialRatioAuditLogs, 
                PhoneToPhonePartRatioAuditLogs, 
                MachinesAuditLogs, 
                MaterialInventoryAuditLogs, 
                PhoneInventoryAuditLogs, 
                RawMaterialAuditLogs, 
                AuditActions,
                OrderItems, 
                Orders, 
                OrderStatus, 
                MaterialInventory, 
                PhoneInventory, 
                PhoneToPhonePartRatio, 
                PhonePartToRawMaterialRatio, 
                PhoneParts, 
                Phone, 
                PhoneBrand, 
                RawMaterial, 
                Machines, 
                Companies, 
                Role 
            RESTART IDENTITY CASCADE;
        ");
    }

    public async Task DisposeAsync() 
    {
        if (Container != null)
        {
            await Container.DisposeAsync();
        }
    }
}
