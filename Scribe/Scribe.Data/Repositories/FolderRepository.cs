using Microsoft.EntityFrameworkCore;
using Scribe.Data.Database;
using Scribe.Data.Model;

namespace Scribe.Data.Repositories;

public class FolderRepository : IRepository<Folder>
{
    public async Task<Folder> Add(Folder folder)
    {
        await using var context = new ScribeContext();
        
        var addedFolder = context.Add(folder);
        await context.SaveChangesAsync();
        
        return addedFolder.Entity;
    }

    public async Task Update(params Folder[] folders)
    {
        await using var context = new ScribeContext();
        
        foreach (var folder in folders)
        {
            var folderEntity = await context.Folders.FindAsync(folder.Id);
            
            if (folderEntity == null) continue;
            
            // A workaround to prevent EF from trying to insert already existing records in the join table 'DocumentTag'.
            // We use the existing folder entity and track the required tags.
            
            folderEntity.Tags.Clear();

            folderEntity.Name = folder.Name;
            folderEntity.NavigationIndex = folder.NavigationIndex;
            
            var tagNames = folder.Tags.Select(tag => tag.Name);
            
            folderEntity.Tags = await context.Tags.Where(
                tag => tag.FolderId == folder.Id && tagNames.Contains(tag.Name)
            ).ToListAsync();
        }
        
        context.ChangeTracker.DetectChanges();
        
        await context.SaveChangesAsync();
    }

    public async Task Delete(Folder[] folders)
    {
        await using var context = new ScribeContext();
        
        context.RemoveRange(folders.AsReadOnly());
        
        await context.SaveChangesAsync();
    }

    public Task<List<Folder>> GetAll()
    {
        using var context = new ScribeContext();
        return context.Folders.ToListAsync();
    }
}

