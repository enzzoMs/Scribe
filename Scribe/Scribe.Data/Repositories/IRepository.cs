namespace Scribe.Data.Repositories;

public interface IRepository<T>
{
    Task<T> Add(T entity);
    Task<T> Update(T entity);
    Task<List<T>> GetAll();
}