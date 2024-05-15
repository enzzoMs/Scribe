namespace Scribe.UI.Events;

public interface IEventAggregator
{
    void Publish<T>(T eventData) where T : IEvent;
    void Subscribe<T>(Action<T> onEvent) where T : IEvent;
    void Unsubscribe<T>(Action<T> onEvent) where T : IEvent;
}