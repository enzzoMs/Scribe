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
    
    public async Task<Document> Update(Document document)
    {
        await using var context = new ScribeContext();

        await context.Folders.FindAsync(document.FolderId);
        
        var updatedDocument = context.Update(document);
        
        await context.SaveChangesAsync();
        
        return updatedDocument.Entity;
    }

    public Task<List<Document>> GetAll()
    {
        using var context = new ScribeContext();
        return context.Documents.ToListAsync();
    }
}