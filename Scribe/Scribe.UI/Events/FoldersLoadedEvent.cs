using Scribe.Data.Model;

namespace Scribe.UI.Events;

public record FoldersLoadedEvent(List<Folder> Folders) : IEvent;