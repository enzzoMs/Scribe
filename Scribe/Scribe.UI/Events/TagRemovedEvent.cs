using Scribe.Data.Model;

namespace Scribe.UI.Events;

public record TagRemovedEvent(Tag RemovedTag) : IEvent;