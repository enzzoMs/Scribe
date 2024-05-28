namespace Scribe.Data.Model;

public class Tag(string name, int id = 0)
{
    public int Id { get; init;  } = id;
    public string Name { get; set; } = name;
}