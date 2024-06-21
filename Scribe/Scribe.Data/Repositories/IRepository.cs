namespace Scribe.Data.Repositories;

public interface IRepository<T>
{
    Task<T> Add(T entity);
    Task Update(params T[] entities);
    Task Delete(params T[] entities);
    Task<List<T>> GetAll();
}