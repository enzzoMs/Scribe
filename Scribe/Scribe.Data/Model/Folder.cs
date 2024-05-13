namespace Scribe.Data.Model;

public class Folder(int id, string name, int index)
{
    public int Id { get; set; } = id;
    public string Name { get; set; } = name;
    public int Index { get; set; } = index;
}