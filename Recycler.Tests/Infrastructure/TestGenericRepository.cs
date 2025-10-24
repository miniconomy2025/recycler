using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text.RegularExpressions;
using Dapper;
using Npgsql;
using Recycler.API;

namespace Recycler.Tests.Infrastructure;

public class TestGenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly string _tableName;
    private readonly string _primaryKeyName = "Id";
    private readonly string _connectionString;
    
    public TestGenericRepository(string connectionString)
    {
        _connectionString = connectionString;
        
        Type entityType = typeof(T);

        _tableName = entityType.GetCustomAttribute<TableAttribute>()?.Name
            ?? throw new NullReferenceException($"{entityType.Name} don't have a TableAttribute");
    }
    
    private NpgsqlConnection GetConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
    
    private string GetColumnNameFromProperty(string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return propertyName;
        }

        return Regex.Replace(propertyName, "(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])", "_$1", RegexOptions.Compiled).ToLower();
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        await using var connection = GetConnection();
        await connection.OpenAsync();
        
        var query = $"SELECT * FROM {_tableName}";
        return await connection.QueryAsync<T>(query);
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        await using var connection = GetConnection();
        await connection.OpenAsync();
        
        var query = $"SELECT * FROM {_tableName} WHERE {_primaryKeyName} = @Id";
        return await connection.QueryFirstOrDefaultAsync<T>(query, new { Id = id });
    }

    public async Task<int> CreateAsync(T entity)
    {
        await using var connection = GetConnection();
        await connection.OpenAsync();
        
        var properties = typeof(T).GetProperties()
            .Where(p => p.Name != _primaryKeyName && p.CanWrite)
            .ToList();

        var columns = string.Join(", ", properties.Select(p => GetColumnNameFromProperty(p.Name)));
        var values = string.Join(", ", properties.Select(p => GetValuePlaceholder(p)));

        var query = $"INSERT INTO {_tableName} ({columns}) VALUES ({values}) RETURNING {_primaryKeyName}";
        
        var parameters = new Dictionary<string, object>();
        foreach (var property in properties)
        {
            var value = property.GetValue(entity);
            var columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
            
            if (columnAttribute?.TypeName == "jsonb" && value is string stringValue)
            {
                parameters[property.Name] = stringValue;
            }
            else
            {
                parameters[property.Name] = value ?? DBNull.Value;
            }
        }
        
        return await connection.QuerySingleAsync<int>(query, parameters);
    }
    
    private string GetValuePlaceholder(PropertyInfo property)
    {
        var columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
        if (columnAttribute?.TypeName == "jsonb")
        {
            return $"@{property.Name}::jsonb";
        }
        return $"@{property.Name}";
    }

    public async Task<IEnumerable<T>> GetByColumnValueAsync(string columnName, object columnValue)
    {
        await using var connection = GetConnection();
        await connection.OpenAsync();
        
        var query = $"SELECT * FROM {_tableName} WHERE {GetColumnNameFromProperty(columnName)} = @Value";
        return await connection.QueryAsync<T>(query, new { Value = columnValue });
    }

    public async Task<IEnumerable<T>> GetByWhereClauseAsync(string condition, object value)
    {
        await using var connection = GetConnection();
        await connection.OpenAsync();
        
        var completeCondition = condition.TrimEnd();
        if (!completeCondition.EndsWith("=") && !completeCondition.EndsWith(">") && !completeCondition.EndsWith("<") && 
            !completeCondition.EndsWith("LIKE") && !completeCondition.EndsWith("IN"))
        {
            completeCondition += " @Value";
        }
        else
        {
            completeCondition += " @Value";
        }
        
        var query = $"SELECT * FROM {_tableName} WHERE {completeCondition}";
        return await connection.QueryAsync<T>(query, new { Value = value });
    }

    public async Task<bool> UpdateAsync(T entity, IEnumerable<string> propertyNamesToUpdate)
    {
        await using var connection = GetConnection();
        await connection.OpenAsync();
        
        var properties = typeof(T).GetProperties()
            .Where(p => p.Name != _primaryKeyName && p.CanWrite && propertyNamesToUpdate.Contains(p.Name))
            .ToList();

        if (!properties.Any())
        {
            return false;
        }

        var setClause = string.Join(", ", properties.Select(p => $"{GetColumnNameFromProperty(p.Name)} = {GetValuePlaceholder(p)}"));
        var primaryKeyValue = typeof(T).GetProperty(_primaryKeyName)?.GetValue(entity);

        var query = $"UPDATE {_tableName} SET {setClause} WHERE {_primaryKeyName} = @{_primaryKeyName}";
        
        var parameters = new Dictionary<string, object>();
        foreach (var property in properties)
        {
            var value = property.GetValue(entity);
            var columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
            
            if (columnAttribute?.TypeName == "jsonb" && value is string stringValue)
            {
                parameters[property.Name] = stringValue;
            }
            else
            {
                parameters[property.Name] = value ?? DBNull.Value;
            }
        }
        parameters[_primaryKeyName] = primaryKeyValue ?? throw new InvalidOperationException("Primary key value is null");

        var rowsAffected = await connection.ExecuteAsync(query, parameters);
        return rowsAffected > 0;
    }
}
