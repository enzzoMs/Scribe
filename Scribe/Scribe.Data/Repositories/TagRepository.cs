using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Scribe.Data.Database;
using Scribe.Data.Model;

namespace Scribe.Data.Repositories;

public class TagRepository : IRepository<Tag>
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    public async Task<List<Tag>> Add(Tag[] tags)
    {
        await using var context = new ScribeContext();

        var addedTags = tags.Select(tag => context.Add(tag).Entity).ToList();

        await context.SaveChangesAsync();
        
        return addedTags;
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
    
    public Task ExportToFile(string directoryPath, Tag tag)
    {
        return File.WriteAllTextAsync(directoryPath, JsonSerializer.Serialize(tag, SerializerOptions));
    }
    
    public async Task<Tag> ImportFromFile(string filePath)
    {
        var tagJson = await File.ReadAllTextAsync(filePath);
        try
        {
            return JsonSerializer.Deserialize<Tag>(tagJson, SerializerOptions)!;
        }
        catch (JsonException e)
        {
            throw new FormatException(
                $"Failed to import entity from file '{filePath}' due to invalid format.", e
            );
        }
    }
}