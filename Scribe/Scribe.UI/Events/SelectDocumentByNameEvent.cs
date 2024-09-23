namespace Scribe.UI.Events;

public record SelectDocumentByNameEvent(string DocumentName) : IEvent;