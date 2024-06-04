using Microsoft.EntityFrameworkCore;
using Scribe.Data.Database;
using Scribe.Data.Model;

namespace Scribe.Data.Repositories;

public class FolderRepository : IRepository<Folder>
{
    public async Task<Folder> Add(Folder folder)
    {
        await using var context = new ScribeContext();
        
        var addedFolder = await context.AddAsync(folder);
        await context.SaveChangesAsync();
        
        return addedFolder.Entity;
    }

    public async Task<Folder> Update(Folder folder)
    {
        await using var context = new ScribeContext();

        var updatedFolder = context.Update(folder);

        await context.SaveChangesAsync();

        return updatedFolder.Entity;
    }

    public Task<List<Folder>> GetAll()
    {
        using var context = new ScribeContext();
        return context.Folders.ToListAsync();
    }
}

