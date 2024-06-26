namespace Scribe.UI.Events;

public record TagSelectionChangedEvent(string TagName, bool IsSelected) : IEvent;