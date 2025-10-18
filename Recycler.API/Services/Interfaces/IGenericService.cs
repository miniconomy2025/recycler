namespace Recycler.API.Services;

public interface IGenericService<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetByColumnValueAsync(string columnName, object columnValue);
    Task<int> CreateAsync(T entity);
    Task<bool> UpdateAsync(T entity, IEnumerable<string> columnNamesToUpdate);
}