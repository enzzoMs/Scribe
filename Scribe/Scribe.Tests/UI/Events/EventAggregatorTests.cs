using Scribe.UI.Events;

namespace Scribe.Tests.UI.Events;

internal class TestEvent : IEvent;

public class EventAggregatorTests
{
    private readonly EventAggregator _eventAggregator = new();
    
    [Fact]
    public void SubscribesAndPublishesEvent()
    {
        var eventHandled = false;
        
        _eventAggregator.Subscribe<TestEvent>(_ => eventHandled = true);
        _eventAggregator.Publish(new TestEvent());
        
        Assert.True(eventHandled);
    }
    
    [Fact]
    public void UnsubscribesFromEvent()
    {
        var eventHandled = false;
        Action<TestEvent> handler = _ => eventHandled = true;
        
        _eventAggregator.Subscribe(handler);
        _eventAggregator.Unsubscribe(handler);
        _eventAggregator.Publish(new TestEvent());
        
        Assert.False(eventHandled);
    }
    
    [Fact]
    public void EventDataIsReceived()
    {
        TestEvent testEvent = new();
        IEvent? receivedEvent = null;
        
        _eventAggregator.Subscribe<TestEvent>(e => receivedEvent = e);
        _eventAggregator.Publish(testEvent);

        Assert.NotNull(receivedEvent);
        Assert.IsType<TestEvent>(receivedEvent);
        Assert.Same(testEvent, receivedEvent);
    }
}