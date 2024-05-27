using Scribe.Data.Model;

namespace Scribe.UI.Events;

public record FolderSelectedEvent(Folder? Folder) : IEvent;