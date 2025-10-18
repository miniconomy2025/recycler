namespace Recycler.API;

using System.Collections.Generic;

public interface IGenericRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetByColumnValueAsync(string columnName, object columnValue);
    Task<IEnumerable<T>> GetByWhereClauseAsync(string condition, object value);
    Task<int> CreateAsync(T entity);
    Task<bool> UpdateAsync(T entity, IEnumerable<string> propertyNamesToUpdate);
}