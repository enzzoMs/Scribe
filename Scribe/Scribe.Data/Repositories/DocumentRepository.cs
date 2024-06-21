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
        
        foreach (var document in documents)
        {
            var documentEntity = await context.Documents.FindAsync(document._id);
            
            if (documentEntity == null) continue;
            
            // A workaround to prevent EF from trying to insert already existing records in the join table 'DocumentTag'.
            // We use the existing document entity and only track the required tags.
            
            documentEntity.Tags.Clear();

            foreach (var tag in document.Tags)
            {
                var tagEntity = await context.Tags.FindAsync(tag.Name, tag.FolderId);

                if (tagEntity != null)
                {
                    documentEntity.Tags.Add(tagEntity);
                }
            }
            context.Documents.Update(documentEntity);   
        }
        
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