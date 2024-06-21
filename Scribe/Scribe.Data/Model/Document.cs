namespace Scribe.Data.Model;

public class Document(
    int folderId,
    DateTime createdTimestamp,
    DateTime lastModifiedTimestamp,
    string name = "")
{
    public int _id { get; private set; }    
    public string Name { get; set; } = name;
    public string Content { get; set; } = "";
    public bool IsPinned { get; set; }
    public DateTime LastModifiedTimestamp { get; set; } = lastModifiedTimestamp;
    public DateTime CreatedTimestamp { get; init; } = createdTimestamp;
    public ICollection<Tag> Tags { get; } = [];
    public int FolderId { get; } = folderId;

    public override string ToString() => Name;
}