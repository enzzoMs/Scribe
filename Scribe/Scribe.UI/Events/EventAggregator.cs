using System.Runtime.CompilerServices;

namespace Scribe.UI.Events;

public class EventAggregator : IEventAggregator
{
    private readonly ConditionalWeakTable<object, Dictionary<Type, Delegate>> _eventSubscribers = new();
    
    public void Publish<T>(T eventData) where T : IEvent
    {
        var eventType = typeof(T);

        var eventHandlers = _eventSubscribers
            .Select(subscriberEntry  => subscriberEntry.Value)
            .SelectMany(subscriptions => subscriptions)
            .Where(eventHandlerEntry => eventHandlerEntry.Key == eventType || eventType.IsSubclassOf(eventHandlerEntry.Key))
            .Select(eventHandlerEntry  => eventHandlerEntry.Value);

        foreach (var handler in eventHandlers)
        {
            (handler as Action<T>)?.Invoke(eventData);
        }
    }

    public void Subscribe<T>(object subscriber, Action<T> onEvent) where T : IEvent
    {
        var subscriptions = _eventSubscribers.GetOrCreateValue(subscriber);
        
        subscriptions[typeof(T)] = onEvent;
    }

    public void Unsubscribe<T>(object subscriber) where T : IEvent
    {
        var eventType = typeof(T);
        
        var subscriptions = _eventSubscribers.GetOrCreateValue(subscriber);
        
        subscriptions.Remove(eventType);
    }
}