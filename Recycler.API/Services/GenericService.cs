namespace Recycler.API.Services;

public class GenericService<T>(IGenericRepository<T> repository) : IGenericService<T> where T : class
{
    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await repository.GetAllAsync();
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<T>> GetByColumnValueAsync(string columnName, object columnValue)
    {
        return await repository.GetByColumnValueAsync(columnName, columnValue);
    }
    
    public async Task<int> CreateAsync(T entity)
    {
        return await repository.CreateAsync(entity);
    }

    public async Task<bool> UpdateAsync(T entity, IEnumerable<string> columnNamesToUpdate)
    {
        return await repository.UpdateAsync(entity, columnNamesToUpdate);
    }
}