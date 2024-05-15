namespace Scribe.UI.Events;

public class EventAggregator : IEventAggregator
{
    private readonly Dictionary<Type, List<WeakReference>> _eventSubscribers = new();
    
    public void Publish<T>(T eventData) where T : IEvent
    {
        var eventType = typeof(T);

        if (!_eventSubscribers.TryGetValue(eventType, out var subscribers))
            return;
        
        var eventCallbacks = subscribers.Where(onEvent => onEvent.IsAlive);
            
        foreach (var eventReference in eventCallbacks)
        {
            (eventReference.Target as Action<T>)?.Invoke(eventData);
        }
    }

    public void Subscribe<T>(Action<T> onEvent) where T : IEvent
    {
        var eventType = typeof(T);

        if (!_eventSubscribers.ContainsKey(eventType))
        {
            _eventSubscribers[eventType] = []; 
        }

        _eventSubscribers[eventType].Add(new WeakReference(onEvent));
    }

    public void Unsubscribe<T>(Action<T> onEvent) where T : IEvent
    {
        var eventType = typeof(T);

        if (_eventSubscribers.TryGetValue(eventType, out var subscribers))
        {
            _eventSubscribers[eventType] = subscribers.Where(
                subscription => !subscription.Target?.Equals(onEvent) ?? true
            ).ToList();;
        }
    }
}