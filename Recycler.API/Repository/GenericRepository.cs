using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text.RegularExpressions;
using Dapper;
using Npgsql;

namespace Recycler.API;


public class GenericRepository<T>: IGenericRepository<T> where T : class
{
    private readonly string _tableName;
    private readonly string _primaryKeyName = "Id";
    private readonly IConfiguration _configuration;
    
    public GenericRepository(IConfiguration configuration)
    {
        _configuration = configuration;
        
        Type entityType = typeof(T);

        _tableName = entityType.GetCustomAttribute<TableAttribute>()?.Name
            ?? throw new NullReferenceException($"{entityType.Name} don't have a TableAttribute");
    }
    private NpgsqlConnection GetConnection()
    {
        return new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
    }
    
    private string GetColumnNameFromProperty(string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return propertyName;
        }

        var pattern = @"(?<=[a-z0-9])(?=[A-Z])";
        return Regex.Replace(propertyName, pattern, "_").ToLowerInvariant();
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        await using NpgsqlConnection connection = GetConnection();
        
        return await connection.QueryAsync<T>($"SELECT * FROM {_tableName}");
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        await using NpgsqlConnection connection = GetConnection();
        
        return await connection.QueryFirstOrDefaultAsync<T>($"SELECT * FROM {_tableName} WHERE {_primaryKeyName} = @Id",
            new { Id = id });
    }

    public async Task<IEnumerable<T>> GetByColumnValueAsync(string columnName, object columnValue)
    {
        if (string.IsNullOrEmpty(columnName))
        {
            return [];
        }
    
        await using NpgsqlConnection connection = GetConnection();
        
        return await connection.QueryAsync<T>($"SELECT * FROM {_tableName} WHERE {columnName} = @ColumnValue",
            new { ColumnValue = columnValue });
    }

    public async Task<int> CreateAsync(T entity)
    {
        await using NpgsqlConnection connection = GetConnection();
        
        List<PropertyInfo> properties = typeof(T).GetProperties()
            .Where(p => p.Name != _primaryKeyName)
            .ToList();
        
        Dictionary<string, string> columnAndValues = new Dictionary<string, string>();
        
        properties.ForEach(prop =>
        {
            string columnName = GetColumnNameFromProperty(prop.Name);
            string columnValue = $"@{prop.Name}";
            columnAndValues.Add(columnName, columnValue);
        });
        
        string columns = string.Join(", ", columnAndValues.Keys);
        string parameters = string.Join(", ", columnAndValues.Values);
        
        string sql = $"INSERT INTO {_tableName} ({columns}) VALUES ({parameters}) RETURNING {_primaryKeyName};";
        
        return await connection.ExecuteScalarAsync<int>(sql, entity);
    }

    public async Task<bool> UpdateAsync(T entity, IEnumerable<string> propertyNamesToUpdate)
    {
        await using NpgsqlConnection connection = GetConnection();
        

        string setClauses = string.Join(", ", propertyNamesToUpdate.Select(propertyName =>
        {
            PropertyInfo propertyInfo = typeof(T).GetProperty(propertyName) ?? 
                throw new ArgumentException($"Unable to update entity  '{typeof(T).Name}' because it doesn't have a property named '{propertyName}'");

            object columnValue = propertyInfo.GetValue(entity) ?? 
                throw new ArgumentException($"Unable to update entity '{typeof(T).Name}' because '{propertyName}' value is null");

            return $"{GetColumnNameFromProperty(propertyName)} = @{columnValue}";
        }));
            

        PropertyInfo primaryKeyPropertyInfo = typeof(T).GetProperty(_primaryKeyName) ?? 
            throw new ArgumentException($"Unable to update entity  '{typeof(T).Name}' because it doesn't have a primary key property named '{_primaryKeyName}'");
        
        object primaryKeyValue = primaryKeyPropertyInfo.GetValue(entity) ?? 
            throw new ArgumentException($"Unable to update entity '{typeof(T).Name}' because primary key is null");

        var sql = $"UPDATE {_tableName} SET {setClauses} WHERE {_primaryKeyName} = @{primaryKeyValue}";
        
        return await connection.ExecuteAsync(sql, entity) > 0;
    }
}