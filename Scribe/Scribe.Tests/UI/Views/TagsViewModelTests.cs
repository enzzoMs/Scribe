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
    public void ToggleTagSelectionCommand_Toggles_TagSelection()
    {
        var folder = new Folder("", 0);
        var tagA = new Tag("TagA", 0);
        folder.Tags.Add(tagA);
        
        _eventAggregator.Publish(new FolderSelectedEvent(folder));

        _tagsViewModel.ToggleTagSelectionCommand.Execute(_tagsViewModel.Tags![0]);
        Assert.True(_tagsViewModel.Tags[0].IsSelected);
        _tagsViewModel.ToggleTagSelectionCommand.Execute(_tagsViewModel.Tags[0]);
        Assert.False(_tagsViewModel.Tags[0].IsSelected);
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

        _tagsViewModel.ToggleTagSelectionCommand.Execute(_tagsViewModel.Tags![0]);
        
        Assert.NotNull(tagSelectionEvent);
        Assert.Equal(tag.Name, tagSelectionEvent.TagName);
        Assert.True(tagSelectionEvent.IsSelected);
    }
    
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

        Assert.NotNull(_tagsViewModel.Tags);
        Assert.Equal(3, _tagsViewModel.Tags.Count);
        Assert.Equal("TagA", _tagsViewModel.Tags[0].Name);
        Assert.Equal("TagB", _tagsViewModel.Tags[1].Name);
        Assert.Equal("TagC", _tagsViewModel.Tags[2].Name);
    }

    [Fact]
    public void TagAddedEvent_AddsTagItem()
    {
        var folder = new Folder("", 0);
        _eventAggregator.Publish(new FolderSelectedEvent(folder));
        
        var tag = new Tag("Tag", 0);
        _eventAggregator.Publish(new TagAddedEvent(tag));

        Assert.Single(_tagsViewModel.Tags!);
        Assert.Equal(tag.Name, _tagsViewModel.Tags![0].Name);
    }

    [Fact]
    public void TagAddedEvent_IgnoresTag_IfAlreadyAdded()
    {
        var folder = new Folder("", 0);
        _eventAggregator.Publish(new FolderSelectedEvent(folder));

        var tag = new Tag("Tag", 0);
        _eventAggregator.Publish(new TagAddedEvent(tag));
        _eventAggregator.Publish(new TagAddedEvent(tag));

        Assert.Single(_tagsViewModel.Tags!);
        Assert.Equal(tag.Name, _tagsViewModel.Tags![0].Name);
    }
    
    [Fact]
    public void TagAddedEvent_IgnoresTag_IfItBelongsToAnotherFolder()
    {
        var folder = new Folder("", 0);
        _eventAggregator.Publish(new FolderSelectedEvent(folder));

        var tag = new Tag("Tag", folderId: 1);
        _eventAggregator.Publish(new TagAddedEvent(tag));

        Assert.Empty(_tagsViewModel.Tags!);
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

        Assert.Empty(_tagsViewModel.Tags!);
    }
    
    [Fact]
    public void TagRemovedEvent_IgnoresTag_IfItBelongsToAnotherFolder()
    {
        var folder = new Folder("", 0);
        var tag = new Tag("Tag",  folderId: 0);
        folder.Tags.Add(tag);
        
        _eventAggregator.Publish(new FolderSelectedEvent(folder));
        
        _eventAggregator.Publish(new TagRemovedEvent(new Tag("Tag", folderId: 1)));

        Assert.Single(_tagsViewModel.Tags!);
        Assert.Equal(tag.Name, _tagsViewModel.Tags![0].Name);
    }
    
    [Fact]
    public void TagRemovedEvent_IgnoresTag_IfIsStillUsed()
    {
        var folder = new Folder("", 0);
        var tag = new Tag("Tag", 0);
        folder.Tags.Add(tag);
        
        _eventAggregator.Publish(new FolderSelectedEvent(folder));
        _eventAggregator.Publish(new TagRemovedEvent(tag));

        Assert.Single(_tagsViewModel.Tags!);
        Assert.Equal(tag.Name, _tagsViewModel.Tags![0].Name);
    }

    [Fact]
    public void TagRemovedEvent_PublishesTagSelectionEvent_IfTagIsSelected()
    {
        var folder = new Folder("", 0);
        var tag = new Tag("Tag", 0);
        folder.Tags.Add(tag);
        
        _eventAggregator.Publish(new FolderSelectedEvent(folder));

        _tagsViewModel.ToggleTagSelectionCommand.Execute(_tagsViewModel.Tags![0]);
        
        TagSelectionChangedEvent? tagSelectionEvent = null;
        _eventAggregator.Subscribe<TagSelectionChangedEvent>(this, e => tagSelectionEvent = e);
        
        folder.Tags.Remove(tag);
        _eventAggregator.Publish(new TagRemovedEvent(tag));

        Assert.NotNull(tagSelectionEvent);
        Assert.Equal(tag.Name, tagSelectionEvent.TagName);
        Assert.False(tagSelectionEvent.IsSelected);
    }

    [Fact]
    public void TagSelectionEvent_Updates_TagSelectionState()
    {
        var folder = new Folder("", 0);
        var tag = new Tag("Tag", 0);
        folder.Tags.Add(tag);
        
        _eventAggregator.Publish(new FolderSelectedEvent(folder));
        
        _eventAggregator.Publish(new TagSelectionChangedEvent(tag.Name, IsSelected: true));
        Assert.True(_tagsViewModel.Tags![0].IsSelected);
        
        _eventAggregator.Publish(new TagSelectionChangedEvent(tag.Name, IsSelected: false));
        Assert.False(_tagsViewModel.Tags![0].IsSelected);
    }
}