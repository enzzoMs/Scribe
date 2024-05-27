using Microsoft.EntityFrameworkCore;
using Scribe.Data.Database;

namespace Scribe.Data.Repositories;

public abstract class GenericRepository<T> : IRepository<T> where T : class
{
    public async Task<T> Add(T entity)
    {
        await using var context = new ScribeContext();
        
        var addedEntity = await context.AddAsync(entity);
        await context.SaveChangesAsync();
        
        return addedEntity.Entity;
    }

    public async Task<T> Update(T entity)
    {
        await using var context = new ScribeContext();

        var updatedEntity = context.Update(entity);

        await context.SaveChangesAsync();

        return updatedEntity.Entity;
    }

    public Task<List<T>> GetAll()
    {
        using var context = new ScribeContext(); 
        return context.Set<T>().ToListAsync();
    }
}