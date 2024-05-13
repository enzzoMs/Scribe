using Microsoft.EntityFrameworkCore;
using Scribe.Data.Database;

namespace Scribe.Data.Repositories;

public abstract class GenericRepository<T>(ScribeContext context) : IRepository<T> where T : class
{
    private readonly ScribeContext _context = context;

    public Task<List<T>> GetAll() => _context.Set<T>().ToListAsync();

    public Task SaveChanges() => _context.SaveChangesAsync();
}