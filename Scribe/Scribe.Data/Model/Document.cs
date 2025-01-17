﻿using System.Text.Json.Serialization;

namespace Scribe.Data.Model;

public enum DocumentFileFormats { Json, Pdf }

public class Document(
    int folderId,
    DateTime createdTimestamp,
    DateTime lastModifiedTimestamp,
    string name = "")
{
    [JsonIgnore]
    public int Id { get; private set; }    
    
    public string Name { get; set; } = name;
    
    public string Content { get; set; } = "";
    
    [JsonIgnore]
    public bool IsPinned { get; set; }
    
    public DateTime CreatedTimestamp { get; init; } = createdTimestamp;
    
    public DateTime LastModifiedTimestamp { get; set; } = lastModifiedTimestamp;
    
    public ICollection<Tag> Tags { get; set; } = [];
    
    public int FolderId { get; } = folderId;
}