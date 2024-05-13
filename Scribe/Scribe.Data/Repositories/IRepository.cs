namespace Scribe.Data.Repositories;

public interface IRepository<T>
{
    Task<List<T>> GetAll();
    
    Task SaveChanges();
}