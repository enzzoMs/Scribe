using Scribe.Data.Model;
using Scribe.UI.Events;
using Scribe.UI.Views.Sections.Tags;

namespace Scribe.Tests.UI.Views;

public class TagsViewModelTests
{
    private readonly TagsViewModel _tagsViewModel;
    private readonly EventAggregator _eventAggregator = new();

    public TagsViewModelTests() => _tagsViewModel = new TagsViewModel(_eventAggregator);

    [Fact]
    public void FolderSelectedEvent_Sets_TagItems()
    {
        var folder = new Folder("", 0);
        var tagA = new Tag("TagA", 0);
        var tagB = new Tag("TagB", 0);
        var tagC = new Tag("TagC", 0);
        folder.Tags.Add(tagA);
        folder.Tags.Add(tagB);
        folder.Tags.Add(tagC);
        
        _eventAggregator.Publish(new FolderSelectedEvent(folder));

        Assert.Equal(3, _tagsViewModel.TagItems!.Count);
        Assert.Equal("TagA", _tagsViewModel.TagItems[0].Name);
        Assert.Equal("TagB", _tagsViewModel.TagItems[1].Name);
        Assert.Equal("TagC", _tagsViewModel.TagItems[2].Name);
    }

    [Fact]
    public void ToggleTagSelectionCommand_Toggles_TagSelection()
    {
        var folder = new Folder("", 0);
        var tagA = new Tag("TagA", 0);
        folder.Tags.Add(tagA);
        
        _eventAggregator.Publish(new FolderSelectedEvent(folder));

        _tagsViewModel.ToggleTagSelectionCommand.Execute(_tagsViewModel.TagItems![0]);
        Assert.True(_tagsViewModel.TagItems[0].IsSelected);
        _tagsViewModel.ToggleTagSelectionCommand.Execute(_tagsViewModel.TagItems[0]);
        Assert.False(_tagsViewModel.TagItems[0].IsSelected);
    }

    [Fact]
    public void ToggleTagSelectionCommand_Publishes_TagSelectionEvent()
    {
        var folder = new Folder("", 0);
        var tag = new Tag("Tag", 0);
        folder.Tags.Add(tag);
        
        _eventAggregator.Publish(new FolderSelectedEvent(folder));

        TagSelectionChangedEvent? tagSelectionEvent = null;
        _eventAggregator.Subscribe<TagSelectionChangedEvent>(this, e => tagSelectionEvent = e );

        _tagsViewModel.ToggleTagSelectionCommand.Execute(_tagsViewModel.TagItems![0]);
        
        Assert.NotNull(tagSelectionEvent);
        Assert.Equal(tag.Name, tagSelectionEvent.TagName);
        Assert.True(tagSelectionEvent.IsSelected);
    }

    [Fact]
    public void TagAddedEvent_AddsTagItem()
    {
        var folder = new Folder("", 0);
        var tag = new Tag("Tag", 0);
        
        _eventAggregator.Publish(new FolderSelectedEvent(folder));
        _eventAggregator.Publish(new TagAddedEvent(tag));

        Assert.Single(_tagsViewModel.TagItems!);
        Assert.Equal(tag.Name, _tagsViewModel.TagItems![0].Name);
    }

    [Fact]
    public void TagAddedEvent_IgnoresTag_IfAlreadyAdded()
    {
        var folder = new Folder("", 0);
        var tag = new Tag("Tag", 0);
        
        _eventAggregator.Publish(new FolderSelectedEvent(folder));
        
        _eventAggregator.Publish(new TagAddedEvent(tag));
        _eventAggregator.Publish(new TagAddedEvent(tag));

        Assert.Single(_tagsViewModel.TagItems!);
        Assert.Equal(tag.Name, _tagsViewModel.TagItems![0].Name);
    }
    
    [Fact]
    public void TagAddedEvent_IgnoresTag_IfItBelongsToAnotherFolder()
    {
        var folder = new Folder("", 0);
        var tag = new Tag("Tag", folderId: 1);
        
        _eventAggregator.Publish(new FolderSelectedEvent(folder));
        _eventAggregator.Publish(new TagAddedEvent(tag));

        Assert.Empty(_tagsViewModel.TagItems!);
    }

    [Fact]
    public void TagRemovedEvent_RemovesTagItem()
    {
        var folder = new Folder("", 0);
        var tag = new Tag("Tag", 0);
        folder.Tags.Add(tag);
        
        _eventAggregator.Publish(new FolderSelectedEvent(folder));
        
        folder.Tags.Remove(tag);
        _eventAggregator.Publish(new TagRemovedEvent(tag));

        Assert.Empty(_tagsViewModel.TagItems!);
    }
    
    [Fact]
    public void TagRemovedEvent_IgnoresTag_IfItBelongsToAnotherFolder()
    {
        var folder = new Folder("", 0);
        var tag = new Tag("Tag",  folderId: 0);
        folder.Tags.Add(tag);
        
        _eventAggregator.Publish(new FolderSelectedEvent(folder));
        folder.Tags.Remove(tag);
        
        _eventAggregator.Publish(new TagRemovedEvent(new Tag("Tag", folderId: 1)));

        Assert.Single(_tagsViewModel.TagItems!);
        Assert.Equal(tag.Name, _tagsViewModel.TagItems![0].Name);
    }
    
    [Fact]
    public void TagRemovedEvent_IgnoresTag_IfIsStillUsed()
    {
        var folder = new Folder("", 0);
        var tag = new Tag("Tag", 0);
        folder.Tags.Add(tag);
        
        _eventAggregator.Publish(new FolderSelectedEvent(folder));
        _eventAggregator.Publish(new TagRemovedEvent(tag));

        Assert.Single(_tagsViewModel.TagItems!);
        Assert.Equal(tag.Name, _tagsViewModel.TagItems![0].Name);
    }

    [Fact]
    public void TagRemovedEvent_PublishesTagSelectionEvent_IfTagIsSelected()
    {
        var folder = new Folder("", 0);
        var tag = new Tag("Tag", 0);
        folder.Tags.Add(tag);
        
        _eventAggregator.Publish(new FolderSelectedEvent(folder));

        _tagsViewModel.ToggleTagSelectionCommand.Execute(_tagsViewModel.TagItems![0]);
        
        TagSelectionChangedEvent? tagSelectionEvent = null;
        _eventAggregator.Subscribe<TagSelectionChangedEvent>(this, e => tagSelectionEvent = e);
        
        folder.Tags.Remove(tag);
        _eventAggregator.Publish(new TagRemovedEvent(tag));

        Assert.NotNull(tagSelectionEvent);
        Assert.Equal(tag.Name, tagSelectionEvent.TagName);
        Assert.False(tagSelectionEvent.IsSelected);
    }
}