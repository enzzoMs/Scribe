using Scribe.Data.Model;

namespace Scribe.UI.Events;

public record TagAddedEvent(Tag CreatedTag) : IEvent;