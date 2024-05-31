namespace Scribe.UI.Events;

public record FolderUpdatedEvent(int UpdatedFolderId) : IEvent;

public record FolderPositionUpdatedEvent(
    int UpdatedFolderId, int OldIndex, int NewIndex
) : FolderUpdatedEvent(UpdatedFolderId);