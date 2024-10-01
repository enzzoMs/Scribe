using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Scribe.Data.Database;
using Scribe.Data.Model;

namespace Scribe.Data.Repositories;

public class DocumentRepository : IRepository<Document>
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };
    
    public async Task<List<Document>> Add(Document[] documents)
    {
        await using var context = new ScribeContext();
        
        var distinctTags = documents
            .Select(doc => doc.Tags)
            .SelectMany(tags => tags)
            .GroupBy(tag => (tag.FolderId, tag.Name))
            .Select(tagGroup => tagGroup.First());

        foreach (var tag in distinctTags)
        {
            context.Tags.Attach(tag);
        }
        
        foreach (var document in documents)
        {
            context.Entry(document).State = EntityState.Added;
        }
        
        await context.SaveChangesAsync();
        
        return documents.ToList();
    }
    
    public async Task Update(Document[] documents)
    {
        await using var context = new ScribeContext();
        
        foreach (var document in documents)
        {
            var documentEntity = await context.Documents.FindAsync(document.Id);

            if (documentEntity == null) continue;

            documentEntity.Name = document.Name;
            documentEntity.Content = document.Content;
            documentEntity.IsPinned = document.IsPinned;
            documentEntity.LastModifiedTimestamp = document.LastModifiedTimestamp;
            
            var tagNames = document.Tags.Select(tag => tag.Name);

            documentEntity.Tags = await context.Tags.Where(
                tag => tag.FolderId == document.FolderId && tagNames.Contains(tag.Name)
            ).ToListAsync();
        }
        
        await context.SaveChangesAsync();
    }
    
    public async Task Delete(Document[] documents)
    {
        await using var context = new ScribeContext();
        
        var documentsId = documents.Select(document => document.Id).ToList();
        
        context.RemoveRange(context.Documents.Where(document => documentsId.Contains(document.Id)));
        
        await context.SaveChangesAsync();
    }

    public Task<List<Document>> GetAll()
    {
        using var context = new ScribeContext();
        return context.Documents.AsNoTracking().ToListAsync();
    }

    public Task ExportToFile(string directoryPath, Document document)
    {
        var filePath = $"{directoryPath}/{document.Name}.json";
        var fileNumber = 1;
        
        while (File.Exists(filePath))
        {
            filePath = $"{directoryPath}/{document.Name} ({fileNumber}).json";
            fileNumber++;
        }
        
        return File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(document, SerializerOptions));
    }

    public async Task<Document> ImportFromFile(string filePath)
    {
        var documentJson = await File.ReadAllTextAsync(filePath);
        try
        {
            return JsonSerializer.Deserialize<Document>(documentJson, SerializerOptions)!;
        }
        catch (JsonException e)
        {
            throw new FormatException(
                $"Failed to import entity from file '{filePath}' due to invalid format.", e
            );
        }
    }
}