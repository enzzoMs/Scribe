using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Scribe.Data.Database;
using Scribe.Data.Model;

namespace Scribe.Data.Repositories;

public class FolderRepository(IRepository<Document> documentsRepository) : IRepository<Folder>
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    public async Task<List<Folder>> Add(Folder[] folders)
    {
        await using var context = new ScribeContext();

        var addedFolders = new List<Folder>();
        
        foreach (var folder in folders)
        {
            context.AttachRange(folder.Documents);
            addedFolders.Add(context.Add(folder).Entity);
        }
        
        await context.SaveChangesAsync();
        
        return addedFolders;
    }

    public async Task Update(params Folder[] folders)
    {
        await using var context = new ScribeContext();
        
        foreach (var folder in folders)
        {
            var folderEntity = await context.Folders.FindAsync(folder.Id);
            
            if (folderEntity == null) continue;
            
            folderEntity.Name = folder.Name;
            folderEntity.NavigationIndex = folder.NavigationIndex;
            
            var tagNames = folder.Tags.Select(tag => tag.Name);
            
            folderEntity.Tags = await context.Tags.Where(
                tag => tag.FolderId == folder.Id && tagNames.Contains(tag.Name)
            ).ToListAsync();
        }
        
        await context.SaveChangesAsync();
    }

    public async Task Delete(Folder[] folders)
    {
        await using var context = new ScribeContext();

        var foldersId = folders.Select(folder => folder.Id).ToList();
        
        context.RemoveRange(context.Folders.Where(folder => foldersId.Contains(folder.Id)));
        
        await context.SaveChangesAsync();
    }

    public Task<List<Folder>> GetAll()
    {
        using var context = new ScribeContext();
        return context.Folders.AsNoTracking().ToListAsync();
    }
    
    public async Task ExportToFile(string directoryPath, Folder folder)
    {
        if (!Directory.Exists(directoryPath))
        {
            throw new DirectoryNotFoundException($"Directory '{directoryPath}' does not exist.");
        }
        
        var folderPath = $"{directoryPath}/{folder.Name}";
        var folderNumber = 1;
        
        while (Directory.Exists(folderPath))
        {
            folderPath = $"{directoryPath}/{folder.Name} ({folderNumber})";
            folderNumber++;
        }
        
        Directory.CreateDirectory(folderPath);
        
        foreach (var document in folder.Documents)
        {
            await documentsRepository.ExportToFile(folderPath, document);
        }
    }
    
    public async Task<Folder> ImportFromFile(string filePath)
    {
        var documentJson = await File.ReadAllTextAsync(filePath);
        try
        {
            return JsonSerializer.Deserialize<Folder>(documentJson, SerializerOptions)!;
        }
        catch (JsonException e)
        {
            throw new FormatException(
                $"Failed to import entity from file '{filePath}' due to invalid format.", e
            );
        }
    }
}

