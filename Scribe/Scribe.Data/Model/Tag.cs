namespace Scribe.Data.Model;

public class Tag(string name, int folderId)
{
    public string Name { get; set; } = name;
    public int FolderId { get; } = folderId;
}