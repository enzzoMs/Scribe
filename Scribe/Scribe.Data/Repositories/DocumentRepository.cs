using Microsoft.EntityFrameworkCore;
using Scribe.Data.Database;
using Scribe.Data.Model;

namespace Scribe.Data.Repositories;

public class DocumentRepository : IRepository<Document>
{
    public async Task<Document> Add(Document document)
    {
        await using var context = new ScribeContext();

        await context.Folders.FindAsync(document.FolderId);
        
        var addedDocument = await context.AddAsync(document);
        
        await context.SaveChangesAsync();
        
        return addedDocument.Entity;
    }
    
    public async Task Update(Document[] documents)
    {
        await using var context = new ScribeContext();

        var associatedFoldersId = documents.Select(d => d.FolderId).Distinct();

        foreach (var folderId in associatedFoldersId)
        {
            await context.Folders.FindAsync(folderId);
        }
        
        foreach (var document in documents)
        {
            context.Update(document);
        }
        
        await context.SaveChangesAsync();
    }
    
    public async Task Delete(Document[] documents)
    {
        await using var context = new ScribeContext();

        foreach (var document in documents)
        {
            context.Remove(document);
        }
        
        await context.SaveChangesAsync();
    }

    public Task<List<Document>> GetAll()
    {
        using var context = new ScribeContext();
        return context.Documents.ToListAsync();
    }
}