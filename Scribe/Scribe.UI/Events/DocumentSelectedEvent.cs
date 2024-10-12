using Scribe.Data.Model;

namespace Scribe.UI.Events;

public record DocumentSelectedEvent(Document SelectedDocument) : IEvent;