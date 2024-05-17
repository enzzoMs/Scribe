namespace Scribe.Data.Model;

public class Folder(string name, int index, int id = 0)
{
    public int Id { get; init; } = id;
    public string Name { get; init; } = name;
    public int Index { get; init; } = index;
}