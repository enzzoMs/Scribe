namespace Scribe.Data.Model;

public class Folder(string name, int navigationPosition, int id = 0)
{
    public int Id { get; init;  } = id;
    public string Name { get; set; } = name;
    public int NavigationPosition { get; set; } = navigationPosition;
    public ICollection<Document> Documents { get; set; } = [];
}