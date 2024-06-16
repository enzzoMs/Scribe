using Scribe.Data.Model;

namespace Scribe.UI.Events;

public record DocumentDeletedEvent(Document DeletedDocument) : IEvent;