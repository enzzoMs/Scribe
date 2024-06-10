using Scribe.Data.Model;

namespace Scribe.UI.Events;

public record FolderDeletedEvent(Folder DeletedFolder) : IEvent;