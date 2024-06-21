using Microsoft.EntityFrameworkCore;
using Scribe.Data.Database;
using Scribe.Data.Model;

namespace Scribe.Data.Repositories;

public class TagRepository : IRepository<Tag>
{
    public async Task<Tag> Add(Tag tag)
    {
        await using var context = new ScribeContext();
        
        var addedTag = context.Add(tag);
        await context.SaveChangesAsync();
        
        return addedTag.Entity;
    }

    public async Task Update(params Tag[] tags)
    {
        await using var context = new ScribeContext();
        
        context.UpdateRange(tags.AsReadOnly());
        
        await context.SaveChangesAsync();
    }

    public async Task Delete(Tag[] tags)
    {
        await using var context = new ScribeContext();
        
        context.RemoveRange(tags.AsReadOnly());
        
        await context.SaveChangesAsync();
    }

    public Task<List<Tag>> GetAll()
    {
        using var context = new ScribeContext();
        return context.Tags.ToListAsync();
    }
}