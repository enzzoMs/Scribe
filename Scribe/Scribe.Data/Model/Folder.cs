namespace Scribe.Data.Model;

public class Folder(string name, int navigationIndex, int id = 0)
{
    public int Id { get; init;  } = id;
    public string Name { get; set; } = name;
    public int NavigationIndex { get; set; } = navigationIndex;
    public ICollection<Document> Documents { get; set; } = [];
}