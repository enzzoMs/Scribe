namespace Scribe.Data.Repositories;

public interface IRepository<T>
{
    Task<T> Add(T entity);
    Task<List<T>> GetAll();
}