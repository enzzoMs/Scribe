﻿using Microsoft.EntityFrameworkCore;
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

    public async Task Update(params Folder[] folders)
    {
        await using var context = new ScribeContext();
        
        foreach (var folder in folders)
        {
            context.Update(folder);
        }        
        
        await context.SaveChangesAsync();
    }

    public async Task Delete(Folder[] folders)
    {
        await using var context = new ScribeContext();

        foreach (var folder in folders)
        {
            context.Remove(folder);
        }    
        
        await context.SaveChangesAsync();
    }

    public Task<List<Folder>> GetAll()
    {
        using var context = new ScribeContext();
        return context.Folders.ToListAsync();
    }
}

