using Dapper;
using Npgsql;

namespace Recycler.Tests.Infrastructure;

public class TestDatabaseResetService : IDatabaseResetService
{
    private readonly string _connectionString;

    public TestDatabaseResetService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task ResetAsync(CancellationToken cancellationToken)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var resetSql = @"
            -- Disable foreign key checks temporarily
            SET session_replication_role = replica;
            
            -- Delete all data from all tables
            TRUNCATE TABLE 
                Log,
                OrdersAuditLogs,
                PhonePartToRawMaterialRatioAuditLogs,
                PhoneToPhonePartRatioAuditLogs,
                MachinesAuditLogs,
                MaterialInventoryAuditLogs,
                PhoneInventoryAuditLogs,
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
            
            -- Re-enable foreign key checks
            SET session_replication_role = DEFAULT;
            ALTER SEQUENCE IF EXISTS companies_id_seq RESTART WITH 1;
            ALTER SEQUENCE IF EXISTS machines_id_seq RESTART WITH 1;
            ALTER SEQUENCE IF EXISTS phone_id_seq RESTART WITH 1;
            ALTER SEQUENCE IF EXISTS phonebrand_id_seq RESTART WITH 1;
            ALTER SEQUENCE IF EXISTS phoneparts_id_seq RESTART WITH 1;
            ALTER SEQUENCE IF EXISTS rawmaterial_id_seq RESTART WITH 1;
            ALTER SEQUENCE IF EXISTS role_id_seq RESTART WITH 1;
            ALTER SEQUENCE IF EXISTS auditactions_id_seq RESTART WITH 1;
            ALTER SEQUENCE IF EXISTS orders_id_seq RESTART WITH 1;
            ALTER SEQUENCE IF EXISTS orderitems_id_seq RESTART WITH 1;
            ALTER SEQUENCE IF EXISTS materialinventory_id_seq RESTART WITH 1;
            ALTER SEQUENCE IF EXISTS phoneinventory_id_seq RESTART WITH 1;
            ALTER SEQUENCE IF EXISTS phonetophonepartratio_id_seq RESTART WITH 1;
            ALTER SEQUENCE IF EXISTS phoneparttorawmaterialratio_id_seq RESTART WITH 1;
            ALTER SEQUENCE IF EXISTS machines_id_seq RESTART WITH 1;
            ALTER SEQUENCE IF EXISTS companies_id_seq RESTART WITH 1;
            ALTER SEQUENCE IF EXISTS phone_id_seq RESTART WITH 1;
            ALTER SEQUENCE IF EXISTS phonebrand_id_seq RESTART WITH 1;
            ALTER SEQUENCE IF EXISTS phoneparts_id_seq RESTART WITH 1;
            ALTER SEQUENCE IF EXISTS rawmaterial_id_seq RESTART WITH 1;
            ALTER SEQUENCE IF EXISTS role_id_seq RESTART WITH 1;
            ALTER SEQUENCE IF EXISTS auditactions_id_seq RESTART WITH 1;
            ALTER SEQUENCE IF EXISTS orders_id_seq RESTART WITH 1;
            ALTER SEQUENCE IF EXISTS orderitems_id_seq RESTART WITH 1;
            ALTER SEQUENCE IF EXISTS materialinventory_id_seq RESTART WITH 1;
            ALTER SEQUENCE IF EXISTS phoneinventory_id_seq RESTART WITH 1;
            ALTER SEQUENCE IF EXISTS phonetophonepartratio_id_seq RESTART WITH 1;
            ALTER SEQUENCE IF EXISTS phoneparttorawmaterialratio_id_seq RESTART WITH 1;";

        await connection.ExecuteAsync(resetSql);
    }
}
