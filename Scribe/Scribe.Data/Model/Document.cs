namespace Scribe.Data.Model;

public class Document(
    int folderId,
    DateTime createdTimestamp,
    DateTime lastModifiedTimestamp,
    string name = "")
{
    private int _id;
    
    public string Name { get; set; } = name;
    public string Content { get; set; } = "";
    public bool IsFavorite { get; set; }
    public bool IsArchived { get; set; }
    public DateTime LastModifiedTimestamp { get; set; } = lastModifiedTimestamp;
    public DateTime CreatedTimestamp { get; init; } = createdTimestamp;
    public IEnumerable<Tag> Tags { get; } = [];
    public int FolderId { get; } = folderId;

    public override string ToString() => Name;
}