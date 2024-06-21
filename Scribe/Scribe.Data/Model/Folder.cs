namespace Scribe.Data.Model;

public class Folder(
    string name,
    int navigationIndex)
{
    public int Id { get; private set; }
    public string Name { get; set; } = name;
    public int NavigationIndex { get; set; } = navigationIndex;
    public ICollection<Document> Documents { get; } = [];
    public ICollection<Tag> Tags { get; } = [];
}