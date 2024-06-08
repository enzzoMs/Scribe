using Scribe.UI.Events;

namespace Scribe.Tests.UI.Events;

internal class TestEvent : IEvent;
internal class TestEventSubclass : TestEvent;

public class EventAggregatorTests
{
    private readonly EventAggregator _eventAggregator = new();
    
    [Fact]
    public void AllowsToSubscribeAndPublishEvent()
    {
        var eventHandled = false;
        
        _eventAggregator.Subscribe<TestEvent>(this, _ => eventHandled = true);
        _eventAggregator.Publish(new TestEvent());
        
        Assert.True(eventHandled);
    }
    
    [Fact]
    public void SubclassEventIsReceived_When_SubscribedToSuperclass()
    {
        TestEvent? eventData = null;
        
        _eventAggregator.Subscribe<TestEvent>(this, e => eventData = e);
        _eventAggregator.Publish(new TestEventSubclass());
        
        Assert.NotNull(eventData);
        Assert.IsType<TestEventSubclass>(eventData);
    }
    
    [Fact]
    public void AllowsToUnsubscribeFromEvent()
    {
        var eventHandled = false;
        
        _eventAggregator.Subscribe<TestEvent>(this, _ => eventHandled = true);
        _eventAggregator.Unsubscribe<TestEvent>(this);
        _eventAggregator.Publish(new TestEvent());
        
        Assert.False(eventHandled);
    }
    
    [Fact]
    public void PublishedEventDataIsReceived()
    {
        TestEvent testEvent = new();
        IEvent? receivedEvent = null;
        
        _eventAggregator.Subscribe<TestEvent>(this, e => receivedEvent = e);
        _eventAggregator.Publish(testEvent);

        Assert.NotNull(receivedEvent);
        Assert.IsType<TestEvent>(receivedEvent);
        Assert.Same(testEvent, receivedEvent);
    }
}