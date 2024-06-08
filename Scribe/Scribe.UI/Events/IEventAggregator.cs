namespace Scribe.UI.Events;

public interface IEventAggregator
{
    void Publish<T>(T eventData) where T : IEvent;
    void Subscribe<T>(object subscriber, Action<T> eventHandler) where T : IEvent;
    void Unsubscribe<T>(object subscriber) where T : IEvent;
}