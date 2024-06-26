namespace Scribe.Data.Model;

public class Folder(
    string name,
    int navigationIndex)
{
    public int Id { get; private set; }
    public string Name { get; set; } = name;
    public int NavigationIndex { get; set; } = navigationIndex;
    public ICollection<Tag> Tags { get; set; } = [];

    public ICollection<Document> Documents { get; } = [];
}