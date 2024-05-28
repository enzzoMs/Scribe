namespace Scribe.Data.Model;

public class Document(
    string name,
    string content,
    bool isFavorite,
    bool isArchived,
    DateTime createdTimestamp,
    DateTime lastModifiedTimestamp,
    int id = 0)
{
    public int Id { get; init;  } = id;
    public string Name { get; set; } = name;
    public string Content { get; set; } = content;
    public bool IsFavorite { get; set; } = isFavorite;
    public bool IsArchived { get; set; } = isArchived;
    public DateTime CreatedTimestamp { get; init; } = createdTimestamp;
    public DateTime LastModifiedTimestamp { get; set; } = lastModifiedTimestamp;
    public ICollection<Tag> Tags { get; init; } = [];
}