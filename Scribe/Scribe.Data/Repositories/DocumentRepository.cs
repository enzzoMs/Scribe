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
        
        var addedDocument = context.Add(document);
        
        await context.SaveChangesAsync();
        
        return addedDocument.Entity;
    }
    
    public async Task Update(Document[] documents)
    {
        await using var context = new ScribeContext();
        
        context.UpdateRange(documents.AsReadOnly());
        
        await context.SaveChangesAsync();
    }
    
    public async Task Delete(Document[] documents)
    {
        await using var context = new ScribeContext();
        
        context.RemoveRange(documents.AsReadOnly());
        
        await context.SaveChangesAsync();
    }

    public Task<List<Document>> GetAll()
    {
        using var context = new ScribeContext();
        return context.Documents.ToListAsync();
    }
}